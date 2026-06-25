using CMS.Base;
using CMS.Headless;

namespace XperienceCommunity.FusionCache.EventHooks.Headless;

/// <summary>
/// Handles headless item publish events.
/// </summary>
internal sealed class HeadlessItemAfterPublishEventHandler : IAsyncEventHandler<AfterPublishHeadlessItemEvent>
{
    private readonly HeadlessItemCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemAfterPublishEventHandler"/> class.
    /// </summary>
    public HeadlessItemAfterPublishEventHandler(HeadlessItemCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public Task HandleAsync(
        AfterPublishHeadlessItemEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new HeadlessItemEventArgs(asyncEvent.Data),
            nameof(AfterPublishHeadlessItemEvent),
            cancellationToken);
}
