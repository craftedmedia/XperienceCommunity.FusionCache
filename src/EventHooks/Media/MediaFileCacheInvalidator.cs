using CMS.Core;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;

namespace XperienceCommunity.FusionCache.EventHooks.Media;

/// <summary>
/// Invalidates FusionCache entries for media file events.
/// </summary>
internal sealed class MediaFileCacheInvalidator
    : EventCacheInvalidator<MediaFileCacheKeysGenerator, Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileCacheInvalidator"/> class.
    /// </summary>
    public MediaFileCacheInvalidator(
        MediaFileCacheKeysGenerator cacheKeyGenerator,
        DummyCacheKeysService dummyCacheKeysService,
        IEventLogService eventLogService,
        IOptions<XperienceCommunityFusionCacheOptions> options)
        : base(cacheKeyGenerator, dummyCacheKeysService, eventLogService, options)
    {
    }

    /// <inheritdoc />
    protected override string Source => nameof(MediaFileEventHooks);

    /// <inheritdoc />
    protected override bool ShouldInvalidate(Guid mediaFileGuid) =>
        mediaFileGuid != Guid.Empty;
}
