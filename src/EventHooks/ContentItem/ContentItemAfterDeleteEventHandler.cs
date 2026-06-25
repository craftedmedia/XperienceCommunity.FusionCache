using CMS.Base;
using CMS.ContentEngine;

namespace XperienceCommunity.FusionCache.EventHooks.ContentItem;

/// <summary>
/// Handles content item delete events.
/// </summary>
internal sealed class ContentItemAfterDeleteEventHandler : IAsyncEventHandler<AfterDeleteContentItemEvent>
{
    private readonly ContentItemCacheInvalidator invalidator;

    public ContentItemAfterDeleteEventHandler(ContentItemCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    public Task HandleAsync(
        AfterDeleteContentItemEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new ContentItemEventArgs(asyncEvent.Data),
            nameof(AfterDeleteContentItemEvent),
            cancellationToken);
}
