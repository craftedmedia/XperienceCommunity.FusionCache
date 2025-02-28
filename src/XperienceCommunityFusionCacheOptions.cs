using XperienceCommunity.FusionCache.Caching.OutputCache;

using ZiggyCreatures.Caching.Fusion;

namespace XperienceCommunity.FusionCache
{
    /// <summary>
    /// Fusion cache options model.
    /// </summary>
    public class XperienceCommunityFusionCacheOptions
    {
        /// <summary>
        /// Gets or sets the connection string of the redis instance to be used for the L2 cache.
        /// </summary>
        public string? RedisConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the default output cache policy name.
        /// </summary>
        public string OutputCachePolicyName { get; set; } = XperienceCommunityFusionCacheOutputCachePolicy.DefaultPolicyName;

        /// <summary>
        /// Gets or sets the default output cache expiration.
        /// </summary>
        public TimeSpan OutputCacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the default fusion cache entry options.
        /// </summary>
        public FusionCacheEntryOptions? DefaultFusionCacheEntryOptions { get; set; }

        /// <summary>
        /// Gets or sets the default serializer to use.
        /// </summary>
        public string DefaultSerializer { get; set; } = "NewtonsoftJson";
    }
}
