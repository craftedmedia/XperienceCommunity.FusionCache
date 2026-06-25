using CMS.Helpers;

using Microsoft.Extensions.Logging;

using XperienceCommunity.FusionCache.EventHooks.Headless;
using XperienceCommunity.FusionCache.KeyGenerators;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for published headless items.
/// </summary>
internal class HeadlessItemsCacheKeysGenerator : ICacheKeysGenerator<HeadlessItemEventArgs>
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
    /// <param name="headlessItemEventArgs">Published headless item event args.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(HeadlessItemEventArgs headlessItemEventArgs)
    {
        if (headlessItemEventArgs is null)
        {
            logger.LogError("Failed to generate dummy keys for headless item. '{paramName}' was null.", nameof(headlessItemEventArgs));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>();

        // Generate all states set of keys
        set.UnionWith(GetDummyKeys(headlessItemEventArgs, lang: null, allStates: true, includeAllKey: true));

        // Generate non-all states set of keys
        set.UnionWith(GetDummyKeys(headlessItemEventArgs, lang: null, allStates: false, includeAllKey: true));

        // Generate per language keys - for all states
        set.UnionWith(GetDummyKeys(headlessItemEventArgs, lang: headlessItemEventArgs.ContentLanguageName, allStates: true, includeAllKey: false));

        // Generate per language keys - non-all states
        set.UnionWith(GetDummyKeys(headlessItemEventArgs, lang: headlessItemEventArgs.ContentLanguageName, allStates: false, includeAllKey: false));

        return set;
    }

    private static ISet<string> GetDummyKeys(HeadlessItemEventArgs headlessItemEventArgs, string? lang, bool allStates, bool includeAllKey)
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
                        headlessItemEventArgs.ID.ToString(),
                        lang!,
                    }),

            // Include 'byname'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "byname",
                        headlessItemEventArgs.Name,
                        lang!,
                    }),

            // Include 'byguid'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "byguid",
                        headlessItemEventArgs.Guid.ToString(),
                        lang!,
                    }),

            // Include 'bychannel'
            CacheHelper.BuildCacheItemName(
                    new string []
                    {
                        "headlessitem",
                        allStates ? "allstates" : null!,
                        "bychannel",
                        headlessItemEventArgs.HeadlessChannelName,
                        "bycontenttype",
                        headlessItemEventArgs.ContentTypeName,
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
                            headlessItemEventArgs.HeadlessChannelName,
                            "all",
                        }));
        }

        return keys;
    }
}
