using CMS.Base;
using CMS.ContentEngine;

namespace XperienceCommunity.FusionCache.EventHooks.ContentItem;

/// <summary>
/// Handles content item create events.
/// </summary>
internal sealed class ContentItemAfterCreateEventHandler : IAsyncEventHandler<AfterCreateContentItemEvent>
{
    private readonly ContentItemCacheInvalidator invalidator;

    public ContentItemAfterCreateEventHandler(ContentItemCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    public Task HandleAsync(
        AfterCreateContentItemEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new ContentItemEventArgs(asyncEvent.Data),
            nameof(AfterCreateContentItemEvent),
            cancellationToken);
}
