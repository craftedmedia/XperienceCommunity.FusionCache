using CMS.Base;
using CMS.Websites;

namespace XperienceCommunity.FusionCache.EventHooks.WebPage;

/// <summary>
/// Handles web page publish events.
/// </summary>
internal sealed class WebPageAfterPublishEventHandler : IAsyncEventHandler<AfterPublishWebPageEvent>
{
    private readonly WebPageCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageAfterPublishEventHandler"/> class.
    /// </summary>
    public WebPageAfterPublishEventHandler(WebPageCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public Task HandleAsync(
        AfterPublishWebPageEvent asyncEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            new WebPageEventArgs(asyncEvent.Data),
            nameof(AfterPublishWebPageEvent),
            cancellationToken);
}
