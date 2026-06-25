using CMS.Core;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;

namespace XperienceCommunity.FusionCache.EventHooks.Headless;

/// <summary>
/// Invalidates FusionCache entries for headless item events.
/// </summary>
internal sealed class HeadlessItemCacheInvalidator
    : EventCacheInvalidator<HeadlessItemsCacheKeysGenerator, HeadlessItemEventArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemCacheInvalidator"/> class.
    /// </summary>
    public HeadlessItemCacheInvalidator(
        HeadlessItemsCacheKeysGenerator cacheKeyGenerator,
        DummyCacheKeysService dummyCacheKeysService,
        IEventLogService eventLogService,
        IOptions<XperienceCommunityFusionCacheOptions> options)
        : base(cacheKeyGenerator, dummyCacheKeysService, eventLogService, options)
    {
    }

    /// <inheritdoc />
    protected override string Source => nameof(HeadlessItemEventHooks);

    /// <inheritdoc />
    protected override bool ShouldInvalidate(HeadlessItemEventArgs eventArgs) =>
        eventArgs.ID > 0 &&
        eventArgs.Guid != Guid.Empty &&
        !string.IsNullOrWhiteSpace(eventArgs.Name) &&
        !string.IsNullOrWhiteSpace(eventArgs.HeadlessChannelName) &&
        !string.IsNullOrWhiteSpace(eventArgs.ContentTypeName);
}
