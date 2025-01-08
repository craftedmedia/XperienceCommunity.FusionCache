using CMS.Helpers;

using XperienceCommunity.FusionCache.Caching.EventHooks;

using Microsoft.Extensions.Logging;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for content items.
/// </summary>
internal class ContentItemCacheKeysGenerator
{
    private readonly ILogger<ContentItemCacheKeysGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentItemCacheKeysGenerator"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public ContentItemCacheKeysGenerator(ILogger<ContentItemCacheKeysGenerator> logger) => this.logger = logger;

    /// <summary>
    /// Generates dummy keys for a given content item.
    /// </summary>
    /// <param name="contentItemEventArgs">Content item event args.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(ContentItemEventArgs contentItemEventArgs)
    {
        if (contentItemEventArgs is null)
        {
            logger.LogError("Failed to generate dummy keys for content item. '{paramName}' was null.", nameof(contentItemEventArgs));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>();

        // Generate all states set of keys
        set.UnionWith(GetDummyKeys(contentItemEventArgs, lang: null, allStates: true, includeAllKey: true));

        // Generate non-all states set of keys
        set.UnionWith(GetDummyKeys(contentItemEventArgs, lang: null, allStates: false, includeAllKey: true));

        // Generate per language keys - for all states
        set.UnionWith(GetDummyKeys(contentItemEventArgs, lang: contentItemEventArgs.ContentLanguageName, allStates: true, includeAllKey: false));

        // Generate per language keys - non-all states
        set.UnionWith(GetDummyKeys(contentItemEventArgs, lang: contentItemEventArgs.ContentLanguageName, allStates: false, includeAllKey: false));

        return set;
    }

    private static ISet<string> GetDummyKeys(ContentItemEventArgs contentItemEventArgs, string? lang, bool allStates, bool includeAllKey)
    {
        var keys = new HashSet<string>()
        {
            // Include 'byid'
            CacheHelper.BuildCacheItemName(
                    new[]
                    {
                        "contentitem",
                        allStates ? "allstates" : null!,
                        "byid",
                        contentItemEventArgs.ID.ToString(),
                        lang!,
                    }),

            // Include 'byname'
            CacheHelper.BuildCacheItemName(
                    new[]
                    {
                        "contentitem",
                        allStates ? "allstates" : null!,
                        "byname",
                        contentItemEventArgs.Name,
                        lang!,
                    }),

            // Include 'byguid'
            CacheHelper.BuildCacheItemName(
                new[]
                {
                    "contentitem",
                    allStates ? "allstates" : null!,
                    "byguid",
                    contentItemEventArgs.Guid.ToString(),
                    lang!,
                }),

            // Key by content type
            CacheHelper.BuildCacheItemName(
                new[]
                {
                    "contentitem",
                    allStates ? "allstates" : null!,
                    "bycontenttype",
                    contentItemEventArgs.ContentTypeName,
                    lang!,
                }),
        };

        // Include 'all' key (clear everything)
        if (includeAllKey)
        {
            keys.Add(CacheHelper.BuildCacheItemName(
                        new[]
                        {
                            "contentitem",
                            allStates ? "allstates" : null,
                            "all",
                        }));
        }

        return keys;
    }
}
