using CMS.Base;
using CMS.Websites;

namespace XperienceCommunity.FusionCache.EventHooks.WebPage;

/// <summary>
/// Handles web page delete events.
/// </summary>
internal sealed class WebPageAfterDeleteEventHandler : IAsyncEventHandler<AfterDeleteWebPageEvent>
{
    private readonly WebPageCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageAfterDeleteEventHandler"/> class.
    /// </summary>
    public WebPageAfterDeleteEventHandler(WebPageCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public Task HandleAsync(
        AfterDeleteWebPageEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new WebPageEventArgs(asyncEvent.Data),
            nameof(AfterDeleteWebPageEvent),
            cancellationToken);
}
