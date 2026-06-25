using CMS.Core;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;

namespace XperienceCommunity.FusionCache.EventHooks.ContentItem;

/// <summary>
/// Invalidates FusionCache entries for content item events.
/// </summary>
internal sealed class ContentItemCacheInvalidator
    : EventCacheInvalidator<ContentItemCacheKeysGenerator, ContentItemEventArgs>
{
    public ContentItemCacheInvalidator(
        ContentItemCacheKeysGenerator cacheKeyGenerator,
        DummyCacheKeysService dummyCacheKeysService,
        IEventLogService eventLogService,
        IOptions<XperienceCommunityFusionCacheOptions> options)
        : base(cacheKeyGenerator, dummyCacheKeysService, eventLogService, options)
    {
    }

    protected override string Source => nameof(ContentItemEventHooks);
}
