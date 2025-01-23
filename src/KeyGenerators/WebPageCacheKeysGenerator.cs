using CMS.ContentEngine.Internal;
using CMS.Helpers;

using Microsoft.Extensions.Logging;

using XperienceCommunity.FusionCache.Caching.EventHooks;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for webpage items.
/// </summary>
internal class WebPageCacheKeysGenerator
{
    private readonly ILogger<WebPageCacheKeysGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageCacheKeysGenerator"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public WebPageCacheKeysGenerator(ILogger<WebPageCacheKeysGenerator> logger) => this.logger = logger;

    /// <summary>
    /// Generates dummy keys for a given web page item.
    /// </summary>
    /// <param name="webPageEventArgs">Web page event args.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(WebPageEventArgs webPageEventArgs)
    {
        if (webPageEventArgs is null)
        {
            logger.LogError("Failed to generate dummy keys for web page item. '{paramName}' was null.", nameof(webPageEventArgs));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>();

        // Generate all states set of keys
        set.UnionWith(GetDummyKeys(webPageEventArgs, lang: null, allStates: true, includeAllKey: true));

        // Generate non-all states set of keys
        set.UnionWith(GetDummyKeys(webPageEventArgs, lang: null, allStates: false, includeAllKey: true));

        // Generate per language keys - for all states
        set.UnionWith(GetDummyKeys(webPageEventArgs, lang: webPageEventArgs.ContentLanguageName, allStates: true, includeAllKey: false));

        // Generate per language keys - non-all states
        set.UnionWith(GetDummyKeys(webPageEventArgs, lang: webPageEventArgs.ContentLanguageName, allStates: false, includeAllKey: false));

        return set;
    }

    private static ISet<string> GetDummyKeys(WebPageEventArgs webPageEventArgs, string? lang, bool allStates, bool includeAllKey)
    {
        var keys = new HashSet<string>()
        {
            // Include 'byid'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "webpageitem",
                        allStates ? "allstates" : null!,
                        "byid",
                        webPageEventArgs.ID.ToString(),
                        lang!,
                    }),

            // Include 'byname'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "webpageitem",
                        allStates ? "allstates" : null!,
                        "byname",
                        webPageEventArgs.Name,
                        lang!,
                    }),

            // Include 'byguid'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "webpageitem",
                        allStates ? "allstates" : null!,
                        "byguid",
                        webPageEventArgs.Guid.ToString(),
                        lang!,
                    }),

            // Include 'bychannel'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "webpageitem",
                        allStates ? "allstates" : null!,
                        "bychannel",
                        webPageEventArgs.WebsiteChannelName,
                        "bypath",
                        webPageEventArgs.TreePath,
                        lang!,
                    }),
        };

        if (!string.IsNullOrEmpty(webPageEventArgs.ContentTypeName))
        {
            // Include 'bycontenttype'
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "webpageitem",
                            allStates ? "allstates" : null!,
                            "bychannel",
                            webPageEventArgs.WebsiteChannelName,
                            "bycontenttype",
                            webPageEventArgs.ContentTypeName,
                            lang!,
                        }));
        }

        // Include 'childrenofpath'
        foreach (string parentPath in GetParentPaths(webPageEventArgs.TreePath))
        {
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "webpageitem",
                            allStates ? "allstates" : null!,
                            "bychannel",
                            webPageEventArgs.WebsiteChannelName,
                            "childrenofpath",
                            parentPath,
                            lang!,
                        }));
        }

        // Include 'all' key (clear everything)
        if (includeAllKey)
        {
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "webpageitem",
                            allStates ? "allstates" : null!,
                            "all",
                        }));

            // Channel specific version
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "webpageitem",
                            allStates ? "allstates" : null!,
                            "bychannel",
                            webPageEventArgs.WebsiteChannelName,
                            "all",
                        }));
        }

        return keys;
    }

    private static IEnumerable<string> GetParentPaths(string path)
    {
        var hashSet = TreePathUtils.GetTreePathsOnPath(path, true, false).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        return hashSet;
    }
}
