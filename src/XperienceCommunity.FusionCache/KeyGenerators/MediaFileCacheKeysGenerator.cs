using CMS.Helpers;

using Microsoft.Extensions.Logging;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for media file items.
/// </summary>
internal class MediaFileCacheKeysGenerator
{
    private readonly ILogger<MediaFileCacheKeysGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileCacheKeysGenerator"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public MediaFileCacheKeysGenerator(ILogger<MediaFileCacheKeysGenerator> logger) => this.logger = logger;

    /// <summary>
    /// Generates dummy keys for a given media file item.
    /// </summary>
    /// <param name="mediaFileGuid">Guid of the specified media file.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(Guid mediaFileGuid)
    {
        if (mediaFileGuid == Guid.Empty)
        {
            logger.LogError("Failed to generate dummy keys for media file item. '{paramName}' was null.", nameof(mediaFileGuid));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>()
        {
            // Include by guid
            CacheHelper.BuildCacheItemName(new[] { "mediafile", mediaFileGuid.ToString() }),

            // Include by guid and preview
            CacheHelper.BuildCacheItemName(new[] { "mediafile", "preview", mediaFileGuid.ToString() }),
        };

        return set;
    }
}
