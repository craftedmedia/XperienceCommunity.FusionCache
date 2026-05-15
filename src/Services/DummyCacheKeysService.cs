using Microsoft.Extensions.Logging;

using ZiggyCreatures.Caching.Fusion;

namespace XperienceCommunity.FusionCache.Caching.Services;

/// <summary>
/// Service for invalidating FusionCache entries by Kentico-style dummy cache keys.
/// </summary>
internal sealed class DummyCacheKeysService
{
    private readonly IFusionCache fusionCache;
    private readonly ILogger<DummyCacheKeysService> logger;

    public DummyCacheKeysService(
        IFusionCache fusionCache,
        ILogger<DummyCacheKeysService> logger)
    {
        this.fusionCache = fusionCache;
        this.logger = logger;
    }

    /// <summary>
    /// Removes FusionCache entries tagged with the supplied dummy cache keys.
    /// </summary>
    /// <param name="keys">Collection of dummy cache keys used as FusionCache tags.</param>
    public void TouchDummyKeys(IEnumerable<string>? keys)
    {
        if (keys is null)
        {
            return;
        }

        foreach (string key in keys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                logger.LogDebug("Removing FusionCache entries by tag: {Tag}", key);

                fusionCache.RemoveByTag(key);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to remove FusionCache entries by tag: {Tag}",
                    key);
            }
        }
    }
}
