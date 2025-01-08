using ZiggyCreatures.Caching.Fusion;

namespace XperienceCommunity.FusionCache.Caching.Services;

/// <summary>
/// Service for interacting with dummy cache keys.
/// </summary>
internal class DummyCacheKeysService
{
    private readonly IFusionCache fusionCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DummyCacheKeysService"/> class.
    /// </summary>
    /// <param name="fusionCache">Instance of <see cref="IFusionCache"/>.</param>
    public DummyCacheKeysService(IFusionCache fusionCache) => this.fusionCache = fusionCache;

    /// <summary>
    /// Touches Kentico dummy cache keys.
    /// </summary>
    /// <param name="keys">Collection of dummy keys to touch.</param>
    public void TouchDummyKeys(IEnumerable<string> keys)
    {
        if (keys is null)
        {
            return;
        }

        Parallel.ForEach(keys, key => fusionCache.RemoveByTag(key));
    }
}
