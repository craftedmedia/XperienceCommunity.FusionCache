using CMS.Core;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;

namespace XperienceCommunity.FusionCache.EventHooks.WebPage;

/// <summary>
/// Invalidates FusionCache entries for web page events.
/// </summary>
internal sealed class WebPageCacheInvalidator
    : EventCacheInvalidator<WebPageCacheKeysGenerator, WebPageEventArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageCacheInvalidator"/> class.
    /// </summary>
    public WebPageCacheInvalidator(
        WebPageCacheKeysGenerator cacheKeyGenerator,
        DummyCacheKeysService dummyCacheKeysService,
        IEventLogService eventLogService,
        IOptions<XperienceCommunityFusionCacheOptions> options)
        : base(cacheKeyGenerator, dummyCacheKeysService, eventLogService, options)
    {
    }

    /// <inheritdoc />
    protected override string Source => nameof(WebPageEventHooks);

    /// <inheritdoc />
    protected override bool ShouldInvalidate(WebPageEventArgs webPageEventArgs) =>
        webPageEventArgs.ID > 0 &&
        webPageEventArgs.Guid != Guid.Empty &&
        !string.IsNullOrWhiteSpace(webPageEventArgs.WebsiteChannelName) &&
        !string.IsNullOrWhiteSpace(webPageEventArgs.TreePath);
}
