namespace XperienceCommunity.FusionCache.Caching.TagHelpers;

/// <summary>
/// Fusion cache tag helper options.
/// </summary>
public class XperienceFusionCacheTagHelperOptions
{
    /// <summary>
    /// Gets or sets the cache dependency keys.
    /// </summary>
    public string[]? CacheDependencies { get; set; }

    /// <summary>
    /// Gets or sets a collection of optional cachebility rules.
    /// </summary>
    public IEnumerable<Func<ReadOnlyMemory<char>, bool>>? CacheabilityRules { get; set; }

    /// <summary>
    /// Gets or sets the cache duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
