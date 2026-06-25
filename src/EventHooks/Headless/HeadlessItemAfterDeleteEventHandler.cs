using CMS.Base;
using CMS.Headless;

namespace XperienceCommunity.FusionCache.EventHooks.Headless;

/// <summary>
/// Handles headless item delete events.
/// </summary>
internal sealed class HeadlessItemAfterDeleteEventHandler : IAsyncEventHandler<AfterDeleteHeadlessItemEvent>
{
    private readonly HeadlessItemCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemAfterDeleteEventHandler"/> class.
    /// </summary>
    public HeadlessItemAfterDeleteEventHandler(HeadlessItemCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public Task HandleAsync(
        AfterDeleteHeadlessItemEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new HeadlessItemEventArgs(asyncEvent.Data),
            nameof(AfterDeleteHeadlessItemEvent),
            cancellationToken);
}
