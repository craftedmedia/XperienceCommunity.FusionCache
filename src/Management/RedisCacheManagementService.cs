using System.Globalization;
using System.Net;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ZiggyCreatures.Caching.Fusion.Events;

namespace XperienceCommunity.FusionCache.Management;

internal sealed class RedisCacheManagementService(
    RedisCacheManagementOptions options,
    Func<Task<IConnectionMultiplexer>>? connectionFactory,
    Func<IFusionCache> fusionCacheFactory,
    IOptions<FusionCacheOptions> fusionCacheOptions,
    string? redisInstanceName = null) : IRedisCacheManagementService, IDisposable
{
    private const int BatchSize = 128;
    private readonly SemaphoreSlim purgeLock = new(1, 1);

    public bool IsEnabled => connectionFactory is not null;
    public IReadOnlyList<string> ExcludedKeys { get; } = Array.AsReadOnly(options.ExcludedKeys.ToArray());
    public IReadOnlyList<string> ExcludedPatterns { get; } = Array.AsReadOnly(options.ExcludedPatterns.ToArray());

    public async Task<RedisCacheOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        if (connectionFactory is null)
        {
            return new(options.DisplayName, "Disabled (DevMode uses memory caching only)", null, []);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        string? instanceName = string.IsNullOrWhiteSpace(redisInstanceName) ? null : redisInstanceName;
        int? database = null;
        try
        {
            var connection = await connectionFactory().WaitAsync(timeout.Token);
            var configuration = ConfigurationOptions.Parse(connection.Configuration);
            instanceName ??= string.Join(", ", configuration.EndPoints
                .Select(endpoint => endpoint is DnsEndPoint dns ? dns.Host : endpoint.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase));
            database = connection.GetDatabase().Database;
            var servers = GetPrimaryServers(connection);
            var snapshots = await Task.WhenAll(servers.Select(server => ReadServerAsync(server, database.Value, configuration.AllowAdmin, timeout.Token)));
            string status = snapshots.Length == 0 ? "No primary Redis servers connected"
                : snapshots.All(server => server.Connected) ? "Connected"
                : snapshots.Any(server => server.Connected) ? "Partially connected" : "Disconnected";
            return new(options.DisplayName, status, database, snapshots) { InstanceName = instanceName };
        }
        catch (Exception exception) when (IsConnectionFailure(exception))
        {
            return new(options.DisplayName, "Disconnected", database, []) { InstanceName = instanceName };
        }
    }

    public async Task<RedisCachePurgeResult> PurgeAllLayersAsync(CancellationToken cancellationToken = default)
    {
        ValidatePurgeTimeout();

        if (!IsEnabled)
        {
            return new(false, 0, 0, "Purging L1 + L2 is unavailable in DevMode.");
        }

        if (!await purgeLock.WaitAsync(0, cancellationToken))
        {
            return new(false, 0, 0, "A purge is already running on this application instance.");
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.PurgeTimeout);

            var cache = fusionCacheFactory();
            // AddFusionCache() uses the default IOptions instance, not named options.
            var cacheOptions = fusionCacheOptions.Value;

            if (!cache.HasDistributedCache || !cache.HasBackplane || cacheOptions.DisableTagging)
            {
                return new(false, 0, 0,
                    "Purging L1 + L2 requires FusionCache's distributed cache, backplane and tagging to be enabled. Invalidation was not started.");
            }

            await ClearAllLayersAsync(cache, cacheOptions, timeout.Token);

            return new(true, 0, 0,
                "Fusion Cache L1 + L2 invalidation was published to all applications sharing this cache's backplane. " +
                "Remote completion is not acknowledged; disconnected applications may miss the notification.");
        }
        catch (Exception exception) when (IsConnectionFailure(exception) ||
            exception is FusionCacheBackplaneException or FusionCacheDistributedCacheException or FusionCacheSerializationException)
        {
            return new(false, 0, 0,
                "Fusion Cache L1 + L2 invalidation could not be confirmed. " +
                "Some applications may already have invalidated their caches. Check the Redis backplane connection and retry.");
        }
        finally
        {
            purgeLock.Release();
        }
    }

    public async Task<RedisCachePurgeResult> PurgeAsync(IEnumerable<string> excludedKeys,
        IEnumerable<string> excludedPatterns, CancellationToken cancellationToken = default)
    {
        // Validate every rule before connecting or deleting anything. Configuration exclusions are mandatory.
        var exclusions = new RedisKeyExclusions(ExcludedKeys.Concat(excludedKeys), ExcludedPatterns.Concat(excludedPatterns));

        ValidatePurgeTimeout();

        if (connectionFactory is null)
        {
            return new(false, 0, 0, "Redis is disabled in DevMode.");
        }

        if (!await purgeLock.WaitAsync(0, cancellationToken))
        {
            return new(false, 0, 0, "A purge is already running on this application instance.");
        }

        long deleted = 0;
        long excluded = 0;

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.PurgeTimeout);

            var token = timeout.Token;
            var connection = await connectionFactory().WaitAsync(token);
            var database = connection.GetDatabase();
            var servers = GetPrimaryServers(connection);

            if (servers.Length == 0 || servers.Any(server => !server.IsConnected))
            {
                return new(false, 0, 0, "All primary Redis servers must be connected before purging.");
            }

            foreach (var server in servers)
            {
                string cursor = "0";

                do
                {
                    token.ThrowIfCancellationRequested();
                    // Explicit SCAN avoids the KEYS fallback and selects the same database as RedisCache.
                    var response = await server.ExecuteAsync(database.Database, "SCAN",
                        new object[] { cursor, "COUNT", BatchSize }, CommandFlags.DemandMaster).WaitAsync(token);

                    var page = (RedisResult[])response!;
                    cursor = (string)page[0]!;

                    var keys = ((RedisResult[])page[1]!).Select(value => (RedisKey)(byte[])value!).Distinct().ToArray();

                    foreach (var batch in keys.Chunk(BatchSize))
                    {
                        token.ThrowIfCancellationRequested();

                        var pending = new List<Task<RedisResult>>();

                        try
                        {
                            foreach (var key in batch)
                            {
                                if (exclusions.Matches(key))
                                {
                                    excluded++;
                                    continue;
                                }

                                // Individual commands route correctly across cluster hash slots. UNLINK frees memory asynchronously.
                                pending.Add(database.ExecuteAsync("UNLINK", new object[] { key }, CommandFlags.DemandMaster));
                            }
                        }
                        finally
                        {
                            try
                            {
                                // Drain commands already sent before releasing the lock, including synchronous enqueue failures.
                                await Task.WhenAll(pending);
                            }
                            finally
                            {
                                deleted += pending.Where(task => task.IsCompletedSuccessfully).Sum(task => (long)task.Result);
                            }
                        }
                    }
                }
                while (cursor != "0");
            }

            return new(true, deleted, excluded, "Purge completed. Applications may already have created new cache entries.");
        }
        catch (Exception exception) when (IsConnectionFailure(exception))
        {
            return new(false, deleted, excluded,
                "Purge incomplete: Redis was unavailable, a command was denied, or the operation timed out. " +
                "Some keys may already have been deleted; the count includes confirmed deletions only.");
        }
        finally
        {
            purgeLock.Release();
        }
    }

    public void Dispose() => purgeLock.Dispose();

    private void ValidatePurgeTimeout()
    {
        if (options.PurgeTimeout <= TimeSpan.Zero || options.PurgeTimeout > TimeSpan.FromHours(1))
        {
            throw new ArgumentException("RedisManagement:PurgeTimeout must be greater than zero and no more than one hour.");
        }
    }

    private static async Task ClearAllLayersAsync(IFusionCache cache, FusionCacheOptions cacheOptions, CancellationToken token)
    {
        // Keep tag metadata's lifetime rather than using the much shorter normal entry duration.
        var clearOptions = cacheOptions.TagsDefaultEntryOptions.Duplicate();

        clearOptions.AllowBackgroundDistributedCacheOperations = false;
        clearOptions.AllowBackgroundBackplaneOperations = false;
        clearOptions.ReThrowDistributedCacheExceptions = true;
        clearOptions.ReThrowBackplaneExceptions = true;
        clearOptions.ReThrowSerializationExceptions = true;
        clearOptions.SetSkipMemoryCache(false);
        clearOptions.SetSkipDistributedCache(false, false);
        clearOptions.SkipBackplaneNotifications = false;
        clearOptions.SkipDistributedLocker = true;
        clearOptions.DistributedCacheSoftTimeout = Timeout.InfiniteTimeSpan;
        clearOptions.DistributedCacheHardTimeout = Timeout.InfiniteTimeSpan;

        string clearKey = cacheOptions.CacheKeyPrefix + cacheOptions.InternalStrings.TagCacheKeyPrefix +
            cacheOptions.InternalStrings.ClearRemoveTag;
        long startedAt = DateTime.UtcNow.Ticks;
        var published = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnMessagePublished(object? sender, FusionCacheBackplaneMessageEventArgs args)
        {
            var message = args.Message;
            if (message.SourceId == cache.InstanceId && message.CacheKey == clearKey &&
                message.Action == BackplaneMessageAction.EntrySet && message.Timestamp >= startedAt)
            {
                published.TrySetResult();
            }
        }

        cache.Events.Backplane.MessagePublished += OnMessagePublished;
        try
        {
            // false prevents cleared entries from being served again as fail-safe fallbacks.
            await cache.ClearAsync(allowFailSafe: false, options: clearOptions, token: token);
            // Circuit breakers can queue a clear without throwing. Require evidence of publication,
            // allowing time for FusionCache's asynchronous event handlers to run.
            await published.Task.WaitAsync(TimeSpan.FromSeconds(5), token);
        }
        finally
        {
            cache.Events.Backplane.MessagePublished -= OnMessagePublished;
        }
    }

    private static IServer[] GetPrimaryServers(IConnectionMultiplexer connection) => connection.GetEndPoints()
        .Select(endpoint => connection.GetServer(endpoint))
        .Where(server => !server.IsReplica && server.ServerType is ServerType.Standalone or ServerType.Cluster)
        .ToArray();

    private static async Task<RedisServerOverview> ReadServerAsync(IServer server, int database, bool allowAdmin, CancellationToken token)
    {
        long? keys = null;
        string? notice = null;
        var info = new Dictionary<string, string>(StringComparer.Ordinal);
        if (server.IsConnected)
        {
            try
            {
                keys = await server.DatabaseSizeAsync(database, CommandFlags.DemandMaster).WaitAsync(token);
            }
            catch (Exception exception) when (IsConnectionFailure(exception))
            {
                notice = "Key count unavailable (DBSIZE).";
            }

            if (!allowAdmin)
            {
                // StackExchange.Redis rejects INFO locally unless AllowAdmin is enabled.
                notice = (notice + " Server statistics unavailable (INFO): Redis admin mode is disabled.").Trim();
            }
            else
            {
                try
                {
                    var sections = await server.InfoAsync().WaitAsync(token);
                    foreach (var entry in sections.SelectMany(section => section))
                    {
                        info[entry.Key] = entry.Value;
                    }
                }
                catch (Exception exception) when (IsConnectionFailure(exception))
                {
                    notice = (notice + " Server statistics unavailable (INFO).").Trim();
                }
            }
        }
        else
        {
            notice = "Disconnected";
        }

        string endpoint = server.EndPoint is DnsEndPoint dns
            ? $"{dns.Host}:{dns.Port}" : server.EndPoint.ToString() ?? "Unknown";
        return new(endpoint, server.IsConnected, keys,
            info.GetValueOrDefault("redis_version"), ReadNumber(info, "used_memory"),
            ReadNumber(info, "used_memory_peak"), ReadNumber(info, "connected_clients"),
            ReadNumber(info, "uptime_in_seconds"), notice);
    }

    private static long? ReadNumber(Dictionary<string, string> info, string key) =>
        long.TryParse(info.GetValueOrDefault(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out long value) ? value : null;

    private static bool IsConnectionFailure(Exception exception) =>
        // RedisCommandException derives directly from Exception, not RedisException.
        exception is RedisException or RedisCommandException or TimeoutException or OperationCanceledException or InvalidOperationException;
}
