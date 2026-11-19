namespace XperienceCommunity.FusionCache.Management;

/// <summary>Inspects and purges the Redis database used by this package.</summary>
public interface IRedisCacheManagementService
{
    bool IsEnabled { get; }
    IReadOnlyList<string> ExcludedKeys { get; }
    IReadOnlyList<string> ExcludedPatterns { get; }
    Task<RedisCacheOverview> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<RedisCachePurgeResult> PurgeAsync(IEnumerable<string> excludedKeys,
        IEnumerable<string> excludedPatterns, CancellationToken cancellationToken = default);
    /// <summary>
    /// Invalidates all FusionCache entries in L1 and L2 across the shared backplane, without scanning or directly deleting Redis keys.
    /// </summary>
    Task<RedisCachePurgeResult> PurgeAllLayersAsync(CancellationToken cancellationToken = default);
}

/// <summary>A snapshot of the selected database and its primary servers.</summary>
public sealed record RedisCacheOverview(string Name, string Status, int? Database,
    IReadOnlyList<RedisServerOverview> Servers)
{
    /// <summary>The configured cache instance name, or the Redis host names when no name is configured.</summary>
    public string? InstanceName { get; init; }

    public long? TotalKeys => Servers.Count > 0 && Servers.All(server => server.Keys.HasValue)
        ? Servers.Sum(server => server.Keys!.Value) : null;
}

/// <summary>Memory, clients and uptime are server-wide, not limited to the selected database.</summary>
public sealed record RedisServerOverview(string Endpoint, bool Connected, long? Keys,
    string? Version, long? UsedMemoryBytes, long? PeakMemoryBytes, long? ConnectedClients,
    long? UptimeSeconds, string? Notice);

/// <summary>Counts are observations during a scan, not an atomic snapshot.</summary>
public sealed record RedisCachePurgeResult(bool Completed, long DeletedKeys,
    long ExcludedKeyObservations, string Message);
