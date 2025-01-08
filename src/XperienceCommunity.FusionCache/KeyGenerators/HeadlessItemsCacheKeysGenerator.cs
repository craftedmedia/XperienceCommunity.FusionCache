using CMS.Headless;
using CMS.Helpers;

using Microsoft.Extensions.Logging;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for published headless items.
/// </summary>
internal class HeadlessItemsCacheKeysGenerator
{
    private readonly ILogger<HeadlessItemsCacheKeysGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemsCacheKeysGenerator"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public HeadlessItemsCacheKeysGenerator(ILogger<HeadlessItemsCacheKeysGenerator> logger) => this.logger = logger;

    /// <summary>
    /// Generates dummy keys for a given headless item.
    /// </summary>
    /// <param name="publishedHeadlessItemArgs">Published headless item event args.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(PublishHeadlessItemEventArgs publishedHeadlessItemArgs)
    {
        if (publishedHeadlessItemArgs is null)
        {
            logger.LogError("Failed to generate dummy keys for headless item. '{paramName}' was null.", nameof(publishedHeadlessItemArgs));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>();

        // Generate all states set of keys
        set.UnionWith(GetDummyKeys(publishedHeadlessItemArgs, lang: null, allStates: true, includeAllKey: true));

        // Generate non-all states set of keys
        set.UnionWith(GetDummyKeys(publishedHeadlessItemArgs, lang: null, allStates: false, includeAllKey: true));

        // Generate per language keys - for all states
        set.UnionWith(GetDummyKeys(publishedHeadlessItemArgs, lang: publishedHeadlessItemArgs.ContentLanguageName, allStates: true, includeAllKey: false));

        // Generate per language keys - non-all states
        set.UnionWith(GetDummyKeys(publishedHeadlessItemArgs, lang: publishedHeadlessItemArgs.ContentLanguageName, allStates: false, includeAllKey: false));

        return set;
    }

    private static ISet<string> GetDummyKeys(PublishHeadlessItemEventArgs publishedHeadlessItemArgs, string? lang, bool allStates, bool includeAllKey)
    {
        var keys = new HashSet<string>()
        {
            // Include 'byid'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "byid",
                        publishedHeadlessItemArgs.ID.ToString(),
                        lang!,
                    }),

            // Include 'byname'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "byname",
                        publishedHeadlessItemArgs.Name,
                        lang!,
                    }),

            // Include 'byguid'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "byguid",
                        publishedHeadlessItemArgs.Guid.ToString(),
                        lang!,
                    }),

            // Include 'bychannel'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "bychannel",
                        publishedHeadlessItemArgs.HeadlessChannelName,
                        "bycontenttype",
                        publishedHeadlessItemArgs.ContentTypeName,
                        lang!,
                    }),
        };

        // Include 'all' key (clear everything)
        if (includeAllKey)
        {
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "headlessitem",
                            allStates ? "allstates" : null!,
                            "all",
                        }));

            // Channel specific version
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "headlessitem",
                            allStates ? "allstates" : null!,
                            "bychannel",
                            publishedHeadlessItemArgs.HeadlessChannelName,
                            "all",
                        }));
        }

        return keys;
    }
}
