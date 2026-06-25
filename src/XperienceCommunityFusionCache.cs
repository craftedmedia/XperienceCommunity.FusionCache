using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using StackExchange.Redis;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.OutputCache;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.EventHooks.ContentItem;
using XperienceCommunity.FusionCache.EventHooks.General;
using XperienceCommunity.FusionCache.EventHooks.Headless;
using XperienceCommunity.FusionCache.EventHooks.Media;
using XperienceCommunity.FusionCache.EventHooks.Settings;
using XperienceCommunity.FusionCache.EventHooks.WebPage;
using XperienceCommunity.FusionCache.Services;

using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed.Redis;
using ZiggyCreatures.Caching.Fusion.Serialization;
using ZiggyCreatures.Caching.Fusion.Serialization.CysharpMemoryPack;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;
using ZiggyCreatures.Caching.Fusion.Serialization.ProtoBufNet;
using ZiggyCreatures.Caching.Fusion.Serialization.ServiceStackJson;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace XperienceCommunity.FusionCache;

/// <summary>
/// Registers caching XperienceCommunity.FusionCache package.
/// </summary>
public static class XperienceCommunityFusionCache
{
    /// <summary>
    /// Adds Xperience fusion cache services using the Redis connection string configured in the
    /// 'XperienceFusionCache' configuration section.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXperienceFusionCache(
        this IServiceCollection services,
        IConfiguration configuration) => AddXperienceFusionCacheInternal(
            services,
            configuration,
            redisConnectionMultiplexerFactory: null);

    /// <summary>
    /// Adds Xperience fusion cache services using a provided Redis connection multiplexer.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
    /// <param name="redisConnectionMultiplexer">Redis connection multiplexer to use for the distributed cache, backplane, and distributed locker.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXperienceFusionCache(
        this IServiceCollection services,
        IConfiguration configuration,
        IConnectionMultiplexer redisConnectionMultiplexer)
    {
        ArgumentNullException.ThrowIfNull(redisConnectionMultiplexer);

        services.TryAddSingleton(redisConnectionMultiplexer);

        return AddXperienceFusionCacheInternal(
            services,
            configuration,
            serviceProvider => Task.FromResult(serviceProvider.GetRequiredService<IConnectionMultiplexer>()));
    }

    /// <summary>
    /// Adds Xperience fusion cache services using a factory that provides a Redis connection multiplexer.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
    /// <param name="redisConnectionMultiplexerFactory">Factory used to resolve the Redis connection multiplexer. The returned multiplexer is shared and reused.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXperienceFusionCache(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, IConnectionMultiplexer> redisConnectionMultiplexerFactory)
    {
        ArgumentNullException.ThrowIfNull(redisConnectionMultiplexerFactory);

        services.TryAddSingleton(redisConnectionMultiplexerFactory);

        return AddXperienceFusionCacheInternal(
            services,
            configuration,
            serviceProvider => Task.FromResult(serviceProvider.GetRequiredService<IConnectionMultiplexer>()));
    }

    /// <summary>
    /// Adds Xperience fusion cache services using an async factory that provides a Redis connection multiplexer.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
    /// <param name="redisConnectionMultiplexerFactory">Async factory used to resolve the Redis connection multiplexer. The returned multiplexer is shared and reused.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXperienceFusionCache(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, Task<IConnectionMultiplexer>> redisConnectionMultiplexerFactory)
    {
        ArgumentNullException.ThrowIfNull(redisConnectionMultiplexerFactory);

        return AddXperienceFusionCacheInternal(
            services,
            configuration,
            redisConnectionMultiplexerFactory);
    }

    /// <summary>
    /// Uses Xperience fusion cache.
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/> instance.</param>
    /// <returns><see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseXperienceFusionCache(this IApplicationBuilder app)
    {
        app.UseOutputCache();

        return app;
    }

    private static IServiceCollection AddXperienceFusionCacheInternal(
        IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, Task<IConnectionMultiplexer>>? redisConnectionMultiplexerFactory)
    {
        var options = configuration.GetSection("XperienceFusionCache").Get<XperienceCommunityFusionCacheOptions>()
            ?? throw new ArgumentNullException(
                "XperienceFusionCache",
                "No appsettings section found matching expected 'XperienceFusionCache' section.");

        bool hasRedisConnectionString = !string.IsNullOrWhiteSpace(options.RedisConnectionString);
        bool hasProvidedConnectionMultiplexerFactory = redisConnectionMultiplexerFactory is not null;

        if (!options.DevMode && !hasRedisConnectionString && !hasProvidedConnectionMultiplexerFactory)
        {
            throw new ArgumentNullException(
                nameof(XperienceCommunityFusionCacheOptions.RedisConnectionString),
                "A redis connection string or connection multiplexer factory has not been set. " +
                "Please configure RedisConnectionString within the 'XperienceFusionCache' settings section, " +
                "or use an AddXperienceFusionCache overload that provides an IConnectionMultiplexer.");
        }

        options.DefaultFusionCacheEntryOptions ??= new FusionCacheEntryOptions
        {
            // Set some sensible default cache durations.
            Duration = TimeSpan.FromMinutes(10),

            // Normally operations on the distributed cache are executed in a blocking fashion: setting this flag to true lets them run in the background in a kind of fire-and-forget way.
            // This will give a perf boost, but watch out for rare side effects.
            AllowBackgroundDistributedCacheOperations = true,

            // Introduces a bit of randomness, to prevent cache entries from expiring at the same exact time on different nodes.
            JitterMaxDuration = TimeSpan.FromSeconds(2),
        };

        // This will be our primary L1 cache.
        services.AddMemoryCache();

        var fusionCacheBuilder = services
            .AddFusionCache()
            .WithOptions(fusionCacheOptions => fusionCacheOptions.RemoveByTagBehavior = RemoveByTagBehavior.Remove)
            .WithDefaultEntryOptions(options.DefaultFusionCacheEntryOptions)
            .WithSerializer(GetConfiguredSerializer(options.DefaultSerializer));

        if (options.DevMode)
        {
            // Use isolated in-memory cache when in dev mode.
        }
        else if (hasProvidedConnectionMultiplexerFactory)
        {
            // Use the caller-provided connection multiplexer/factory.
            ConfigureRedisUsingConnectionMultiplexerFactory(
                fusionCacheBuilder,
                redisConnectionMultiplexerFactory!);
        }
        else
        {
            // Register a shared app-level Redis connection multiplexer from the configured connection string.
            services.TryAddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(options.RedisConnectionString!));

            ConfigureRedis(
                fusionCacheBuilder,
                serviceProvider => Task.FromResult(
                    serviceProvider.GetRequiredService<IConnectionMultiplexer>()));
        }

        // Register our fusion cache tag helper service.
        services.AddSingleton<FusionCacheTagHelperService>();

        // Register our object cache key generators.
        services.AddSingleton<WebPageCacheKeysGenerator>();
        services.AddSingleton<ContentItemCacheKeysGenerator>();
        services.AddSingleton<HeadlessItemsCacheKeysGenerator>();
        services.AddSingleton<MediaFileCacheKeysGenerator>();
        services.AddSingleton<SettingsKeyCacheKeyGenerator>();
        services.AddSingleton<GeneralObjectCacheKeysGenerator>();
        services.AddSingleton<DummyCacheKeysService>();
        services.AddScoped<CacheVaryByOptionService>();

        // Register our invalidators.
        services.AddSingleton<GeneralObjectCacheInvalidator>();
        services.AddSingleton<ContentItemCacheInvalidator>();
        services.AddSingleton<MediaFileCacheInvalidator>();
        services.AddSingleton<SettingsKeyCacheInvalidator>();
        services.AddSingleton<WebPageCacheInvalidator>();
        services.AddSingleton<HeadlessItemCacheInvalidator>();

        // Register custom fusion cache output cache store and policy.
        services.AddSingleton<IOutputCacheStore, XperienceCommunityFusionCacheOutputCacheStore>();
        services.AddOutputCache(x => x.AddPolicy(
            options.OutputCachePolicyName,
            builder => builder
                .AddPolicy<XperienceCommunityFusionCacheOutputCachePolicy>()
                .Expire(options.OutputCacheExpiration),
            true));

        return services;
    }

    private static void ConfigureRedisUsingConnectionMultiplexerFactory(
        IFusionCacheBuilder fusionCacheBuilder,
        Func<IServiceProvider, Task<IConnectionMultiplexer>> redisConnectionMultiplexerFactory)
    {
        var sharedConnectionMultiplexerFactory =
            CreateSharedConnectionMultiplexerFactory(redisConnectionMultiplexerFactory);

        ConfigureRedis(
            fusionCacheBuilder,
            sharedConnectionMultiplexerFactory);
    }

    private static void ConfigureRedis(
        IFusionCacheBuilder fusionCacheBuilder,
        Func<IServiceProvider, Task<IConnectionMultiplexer>> redisConnectionMultiplexerFactory) => fusionCacheBuilder
            .WithDistributedCache(serviceProvider => new RedisCache(new RedisCacheOptions
            {
                ConnectionMultiplexerFactory = () => redisConnectionMultiplexerFactory(serviceProvider)
            }))
            .WithBackplane(serviceProvider => new RedisBackplane(new RedisBackplaneOptions
            {
                ConnectionMultiplexerFactory = () => redisConnectionMultiplexerFactory(serviceProvider)
            }))
            .WithDistributedLocker(serviceProvider => new RedisDistributedLocker(new RedisDistributedLockerOptions
            {
                ConnectionMultiplexerFactory = () => redisConnectionMultiplexerFactory(serviceProvider)
            }));

    private static Func<IServiceProvider, Task<IConnectionMultiplexer>> CreateSharedConnectionMultiplexerFactory(
        Func<IServiceProvider, Task<IConnectionMultiplexer>> redisConnectionMultiplexerFactory)
    {
        Task<IConnectionMultiplexer>? connectionMultiplexer = null;
        object gate = new();

        return serviceProvider =>
        {
            lock (gate)
            {
                if (connectionMultiplexer is null ||
                    connectionMultiplexer.IsFaulted ||
                    connectionMultiplexer.IsCanceled)
                {
                    connectionMultiplexer = redisConnectionMultiplexerFactory(serviceProvider);
                }

                return connectionMultiplexer;
            }
        };
    }

    private static IFusionCacheSerializer GetConfiguredSerializer(string serializer) => serializer switch
    {
        "NeueccMessagePack" => new FusionCacheNeueccMessagePackSerializer(),
        "CysharpMemoryPack" => new FusionCacheCysharpMemoryPackSerializer(),
        "SystemTextJson" => new FusionCacheSystemTextJsonSerializer(),
        "ServiceStackJson" => new FusionCacheServiceStackJsonSerializer(),
        "ProtoBufNet" => new FusionCacheProtoBufNetSerializer(),
        _ => new FusionCacheNewtonsoftJsonSerializer(),
    };
}
