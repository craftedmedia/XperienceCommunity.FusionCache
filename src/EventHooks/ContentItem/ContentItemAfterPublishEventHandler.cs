using CMS.Base;
using CMS.ContentEngine;

namespace XperienceCommunity.FusionCache.EventHooks.ContentItem;

/// <summary>
/// Handles content item publish events.
/// </summary>
internal sealed class ContentItemAfterPublishEventHandler : IAsyncEventHandler<AfterPublishContentItemEvent>
{
    private readonly ContentItemCacheInvalidator invalidator;

    public ContentItemAfterPublishEventHandler(ContentItemCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    public Task HandleAsync(
        AfterPublishContentItemEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new ContentItemEventArgs(asyncEvent.Data),
            nameof(AfterPublishContentItemEvent),
            cancellationToken);
}
