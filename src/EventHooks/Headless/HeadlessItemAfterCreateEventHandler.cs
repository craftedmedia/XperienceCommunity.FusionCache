using CMS.Base;
using CMS.Headless;

namespace XperienceCommunity.FusionCache.EventHooks.Headless;

/// <summary>
/// Handles headless item create events.
/// </summary>
internal sealed class HeadlessItemAfterCreateEventHandler : IAsyncEventHandler<AfterCreateHeadlessItemEvent>
{
    private readonly HeadlessItemCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemAfterCreateEventHandler"/> class.
    /// </summary>
    public HeadlessItemAfterCreateEventHandler(HeadlessItemCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public Task HandleAsync(
        AfterCreateHeadlessItemEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new HeadlessItemEventArgs(asyncEvent.Data),
            nameof(AfterCreateHeadlessItemEvent),
            cancellationToken);
}
