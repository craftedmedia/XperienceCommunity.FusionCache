using CMS.Helpers;

using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

using XperienceCommunity.FusionCache.Caching.Extensions;
using XperienceCommunity.FusionCache.Extensions;
using XperienceCommunity.FusionCache.OutputCache;
using XperienceCommunity.FusionCache.Services;

namespace XperienceCommunity.FusionCache.Caching.OutputCache;

/// <summary>
/// Kentico fusion cache output cache policy.
/// </summary>
internal class XperienceCommunityFusionCacheOutputCachePolicy : IOutputCachePolicy
{
    public const string DefaultPolicyName = "XperienceFusionCache";

    private readonly IWebPageDataContextRetriever webPageDataContextRetriever;

    /// <summary>
    /// Initializes a new instance of the <see cref="XperienceCommunityFusionCacheOutputCachePolicy"/> class.
    /// </summary>
    /// <param name="webPageDataContextRetriever">Instance of the <see cref="IWebPageDataContextRetriever"/>.</param>
    public XperienceCommunityFusionCacheOutputCachePolicy(IWebPageDataContextRetriever webPageDataContextRetriever) => this.webPageDataContextRetriever = webPageDataContextRetriever;

    /// <summary>
    /// Updates the <see cref="OutputCacheContext"/> before the cache middleware is invoked.
    /// At that point the cache middleware can still be enabled or disabled for the request.
    /// </summary>
    /// <param name="context">The current request's cache context.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns><see cref="ValueTask"/>.</returns>
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        bool attemptOutputCaching = AttemptOutputCaching(context);
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = attemptOutputCaching;
        context.AllowCacheStorage = attemptOutputCaching;
        context.AllowLocking = true;

        // Vary by any query and all route values
        context.CacheVaryByRules.QueryKeys = "*";
        context.CacheVaryByRules.RouteValueNames = "*";

        // Add dynamic vary by option types
        AddVaryByOptionTypes(context.HttpContext, context.CacheVaryByRules.VaryByValues);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Updates the <see cref="OutputCacheContext"/> before the cached response is used.
    /// At that point the freshness of the cached response can be updated.
    /// </summary>
    /// <param name="context">The current request's cache context.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns><see cref="ValueTask"/>.</returns>
    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

    /// <summary>
    /// Updates the <see cref="OutputCacheContext"/> before the response is served and can be cached.
    /// At that point cacheability of the response can be updated.
    /// </summary>
    /// <param name="context">The current request's cache context.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns><see cref="ValueTask"/>.</returns>
    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;

        // Verify existence of cookie headers
        if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
        {
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        // Check response code
        if (response.StatusCode is not StatusCodes.Status200OK and not StatusCodes.Status301MovedPermanently)
        {
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Include dummy key for current page as a standard
        if (webPageDataContextRetriever.TryRetrieve(out var pageData))
        {
            keys.Add(CacheHelper.BuildCacheItemName(
                    new[]
                    {
                        "webpageitem",
                        "byid",
                        pageData.WebPage.WebPageItemID.ToString(),
                        pageData.WebPage.LanguageName,
                    }));
        }

        // Get user defined dummy tags from current HttpContext
        var dummyKeys = context.HttpContext.GetContextDummyKeys();

        if (dummyKeys is not null && dummyKeys.Any())
        {
            keys.UnionWith(dummyKeys);
        }

        // Append tags to output cache context
        context.Tags.UnionWith(keys);

        context.AllowCacheStorage = true;

        return ValueTask.CompletedTask;
    }

    private static bool AttemptOutputCaching(OutputCacheContext context)
    {
        // Check if the current request fulfills the requirements to be cached
        var request = context.HttpContext.Request;

        // Verify the method
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            return false;
        }

        // Skip for preview or edit mode
        if (context.HttpContext.Kentico().Preview().Enabled || context.HttpContext.Kentico().PageBuilder().EditMode)
        {
            return false;
        }

        // Verify existence of authorization headers
        if (!StringValues.IsNullOrEmpty(request.Headers.Authorization) ||
            request.HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            return false;
        }

        return true;
    }

    private void AddVaryByOptionTypes(HttpContext? context, IDictionary<string, string> varyByValues)
    {
        if (context is null)
        {
            return;
        }

        var endpoint = context.GetEndpoint();

        if (endpoint is null)
        {
            return;
        }

        var varyByAttribute = endpoint.Metadata.GetMetadata<XperienceFusionCacheVaryByOptionTypesAttribute>();

        if (varyByAttribute?.VaryByOptionTypes is null)
        {
            return;
        }

        var varyByOptionTypes = context.RequestServices.GetRequiredService<CacheVaryByOptionService>().GetVaryByOptionsDictionary(varyByAttribute.VaryByOptionTypes);

        if (varyByOptionTypes is null || !varyByOptionTypes.Any())
        {
            return;
        }

        varyByValues.AddRange(varyByOptionTypes);
    }
}
