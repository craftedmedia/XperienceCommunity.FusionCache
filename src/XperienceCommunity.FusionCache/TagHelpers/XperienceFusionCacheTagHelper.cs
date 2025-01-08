using System.Globalization;

using CMS.Websites.Routing;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.TagHelpers;

namespace XperienceCommunity.FusionCache.Caching.TagHelpers;
/// <summary>
/// Xperience fusion cache tag helper.
/// </summary>
[HtmlTargetElement("xperience-fusion-cache", Attributes = "name")]
public class XperienceFusionCacheTagHelper : TagHelper
{
    private readonly FusionCacheTagHelperService fusionCacheTagHelperService;
    private readonly IWebsiteChannelContext websiteChannelContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="XperienceFusionCacheTagHelper"/> class.
    /// </summary>
    /// <param name="fusionCacheTagHelperService">Instance of the <see cref="FusionCacheTagHelperService"/>.</param>
    /// <param name="websiteChannelContext">Instance of <see cref="IWebsiteChannelContext"/>.</param>
    public XperienceFusionCacheTagHelper(
        FusionCacheTagHelperService fusionCacheTagHelperService,
        IWebsiteChannelContext websiteChannelContext)
    {
        this.fusionCacheTagHelperService = fusionCacheTagHelperService;
        this.websiteChannelContext = websiteChannelContext;
    }

    /// <summary>
    /// Gets or sets the <see cref="ViewContext"/> for the current executing View.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    /// Gets or sets a unique name to discriminate cached entries.
    /// </summary>
    [HtmlAttributeName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="string" /> to vary the cached result by.
    /// </summary>
    [HtmlAttributeName("vary-by")]
    public string? VaryBy { get; set; }

    /// <summary>
    /// Gets or sets a comma-delimited set of HTTP request headers to vary the cached result by.
    /// </summary>
    [HtmlAttributeName("vary-by-header")]
    public string? VaryByHeader { get; set; }

    /// <summary>
    /// Gets or sets a comma-delimited set of query parameters to vary the cached result by.
    /// </summary>
    [HtmlAttributeName("vary-by-query")]
    public string? VaryByQuery { get; set; }

    /// <summary>
    /// Gets or sets a comma-delimited set of route data parameters to vary the cached result by.
    /// </summary>
    [HtmlAttributeName("vary-by-route")]
    public string? VaryByRoute { get; set; }

    /// <summary>
    /// Gets or sets a comma-delimited set of cookie names to vary the cached result by.
    /// </summary>
    [HtmlAttributeName("vary-by-cookie")]
    public string? VaryByCookie { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cached result is to be varied by the Identity for the logged in
    /// <see cref="HttpContext.User"/>.
    /// </summary>
    [HtmlAttributeName("vary-by-user")]
    public bool VaryByUser { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cached result is to be varied by request culture.
    /// <para>
    /// Setting this to <c>true</c> would result in the result to be varied by <see cref="CultureInfo.CurrentCulture" />
    /// and <see cref="CultureInfo.CurrentUICulture" />.
    /// </para>
    /// </summary>
    [HtmlAttributeName("vary-by-culture")]
    public bool VaryByCulture { get; set; }

    /// <summary>
    /// Gets or sets the duration to cache for.
    /// </summary>
    [HtmlAttributeName("duration")]
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether the tag helper is enabled or not.
    /// </summary>
    [HtmlAttributeName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache dependency keys.
    /// </summary>
    [HtmlAttributeName("cache-dependencies")]
    public string[]? CacheDependencies { get; set; }

    /// <summary>
    /// Gets or sets a collection of cacheability rules to apply to this output cached section.
    /// If any rule returns true, the contents of the tag helper are not cached.
    /// </summary>
    [HtmlAttributeName("cacheability-rules")]
    public IEnumerable<Func<ReadOnlyMemory<char>, bool>>? CacheabilityRules { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        IHtmlContent content;

        if (Enabled && !websiteChannelContext.IsPreview)
        {
            var cacheTagKey = new FusionCacheTagKey(this, context);

            content = await fusionCacheTagHelperService.ProcessContentAsync(output, cacheTagKey, new XperienceFusionCacheTagHelperOptions()
            {
                CacheDependencies = CacheDependencies,
                CacheabilityRules = CacheabilityRules,
                Duration = Duration,
            });
        }
        else
        {
            content = await output.GetChildContentAsync();
        }

        // Clear the contents of the "cache" element since we don't want to render it.
        output.SuppressOutput();

        output.Content.SetHtmlContent(content);
    }
}
