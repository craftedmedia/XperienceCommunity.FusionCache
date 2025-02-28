using Microsoft.AspNetCore.OutputCaching;

using ZiggyCreatures.Caching.Fusion;

namespace XperienceCommunity.FusionCache.Caching.OutputCache;

/// <summary>
/// Xperience fusion cache output cache store.
/// </summary>
internal class XperienceCommunityFusionCacheOutputCacheStore : IOutputCacheStore
{
    private readonly IFusionCache fusionCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="XperienceCommunityFusionCacheOutputCacheStore"/> class.
    /// </summary>
    /// <param name="fusionCache">Instance of the <see cref="IFusionCache"/>.</param>
    public XperienceCommunityFusionCacheOutputCacheStore(IFusionCache fusionCache) => this.fusionCache = fusionCache;

    /// <summary>
    /// Evicts cached responses by tag.
    /// </summary>
    /// <param name="tag">The tag to evict.</param>
    /// <param name="cancellationToken">Indicates that the operation should be cancelled.</param>
    /// <returns><see cref="ValueTask"/>.</returns>
    public async ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken) => await fusionCache.RemoveByTagAsync(tag, token: cancellationToken);

    /// <summary>
    /// Gets the cached response for the given key, if it exists.
    /// If no cached response exists for the given key, <c>null</c> is returned.
    /// </summary>
    /// <param name="key">The cache key to look up.</param>
    /// <param name="cancellationToken">Indicates that the operation should be cancelled.</param>
    /// <returns>The response cache entry if it exists; otherwise <c>null</c>.</returns>
    public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) => await fusionCache.GetOrDefaultAsync<byte[]?>(key, null, token: cancellationToken);

    /// <summary>
    /// Stores the given response in the response cache.
    /// </summary>
    /// <param name="key">The cache key to store the response under.</param>
    /// <param name="value">The response cache entry to store.</param>
    /// <param name="tags">The tags associated with the cache entry to store.</param>
    /// <param name="validFor">The amount of time the entry will be kept in the cache before expiring, relative to now.</param>
    /// <param name="cancellationToken">Indicates that the operation should be cancelled.</param>
    /// <returns><see cref="ValueTask"/>.</returns>
    public async ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken) => await fusionCache.SetAsync(key, value, opts => opts.SetDuration(validFor).SetSize(value.Length), tags: tags, token: cancellationToken);
}
