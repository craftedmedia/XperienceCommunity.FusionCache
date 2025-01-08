using CommonServiceLocator;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.OutputCache;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.Caching.Utilities;

using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace XperienceCommunity.FusionCache;

/// <summary>
/// Registers caching XperienceCommunity.FusionCache package.
/// </summary>
public static class XperienceCommunityFusionCache
{
    /// <summary>
    /// Adds Xperience fusion cache services.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">Instance of <see cref="IConfiguration"/>.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXperienceFusionCache(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("XperienceFusionCache").Get<XperienceCommunityFusionCacheOptions>() ?? throw new ArgumentNullException("XperienceFusionCache", "No appsettings section found matching expected 'XperienceFusionCache' section.");

        if (string.IsNullOrEmpty(options.RedisConnectionString))
        {
            throw new ArgumentNullException(nameof(XperienceCommunityFusionCacheOptions.RedisConnectionString), "A redis connection string has not been set. Please configure one within the 'XperienceFusionCache' settings section.");
        }

        options.DefaultFusionCacheEntryOptions ??= new FusionCacheEntryOptions
        {
            // Set some sensible default cache durations.
            Duration = TimeSpan.FromMinutes(10),
            DistributedCacheDuration = TimeSpan.FromMinutes(10),

            // Allows returning of stale cache items if we lose connectivity to the DB for example.
            IsFailSafeEnabled = true,
            FailSafeMaxDuration = TimeSpan.FromHours(2),
            FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

            // Controls timeouts for factory methods. If a method times out then we will just return a stale value.
            FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
            FactoryHardTimeout = TimeSpan.FromSeconds(5),

            // Set distributed cache timeouts. If a request times out, returns stale value.
            DistributedCacheSoftTimeout = TimeSpan.FromSeconds(1),
            DistributedCacheHardTimeout = TimeSpan.FromSeconds(2),
            AllowBackgroundDistributedCacheOperations = true,

            // Introduces a bit of randomness, to prevent cache entries from expiring at the same exact time on different nodes.
            JitterMaxDuration = TimeSpan.FromSeconds(2),
        };

        // This will be our primary L1 cache.
        services.AddMemoryCache();

        // Configure fusion cache, and redis as our L2 cache.
        services.AddFusionCache()
                .WithDefaultEntryOptions(options.DefaultFusionCacheEntryOptions)
                .WithSerializer(new FusionCacheNewtonsoftJsonSerializer())
                .WithDistributedCache(new RedisCache(new RedisCacheOptions() { Configuration = options.RedisConnectionString }))
                .WithBackplane(new RedisBackplane(new RedisBackplaneOptions() { Configuration = options.RedisConnectionString }));

        // Register our fusion cache tag helper service
        services.AddSingleton<FusionCacheTagHelperService>();

        // Register our object cache key generators
        services.AddSingleton<WebPageCacheKeysGenerator>();
        services.AddSingleton<ContentItemCacheKeysGenerator>();
        services.AddSingleton<HeadlessItemsCacheKeysGenerator>();
        services.AddSingleton<MediaFileCacheKeysGenerator>();
        services.AddSingleton<SettingsKeyCacheKeyGenerator>();
        services.AddSingleton<GeneralObjectCacheKeysGenerator>();
        services.AddSingleton<DummyCacheKeysService>();

        // Register custom fusion cache output cache store and policy
        services.AddSingleton<IOutputCacheStore, XperienceCommunityFusionCacheOutputCacheStore>();
        services.AddOutputCache(x => x.AddPolicy(options.OutputCachePolicyName, builder => builder.AddPolicy<XperienceCommunityFusionCacheOutputCachePolicy>().Expire(options.OutputCacheExpiration), true));

        // Configure CSL as can't use DI in Kentico modules :(
        ServiceLocator.SetLocatorProvider(() => new MSServiceLocator(services.BuildServiceProvider()));

        return services;
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
}
