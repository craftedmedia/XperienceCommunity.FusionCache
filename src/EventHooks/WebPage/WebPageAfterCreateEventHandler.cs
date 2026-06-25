using CMS.Base;
using CMS.Websites;

namespace XperienceCommunity.FusionCache.EventHooks.WebPage;

/// <summary>
/// Handles web page create events.
/// </summary>
internal sealed class WebPageAfterCreateEventHandler : IAsyncEventHandler<AfterCreateWebPageEvent>
{
    private readonly WebPageCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageAfterCreateEventHandler"/> class.
    /// </summary>
    public WebPageAfterCreateEventHandler(WebPageCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public Task HandleAsync(
        AfterCreateWebPageEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new WebPageEventArgs(asyncEvent.Data),
            nameof(AfterCreateWebPageEvent),
            cancellationToken);
}
