using CMS.DataEngine;
using CMS.Helpers;

using Microsoft.Extensions.Logging;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for general object items.
/// </summary>
internal class GeneralObjectCacheKeysGenerator
{
    private readonly ILogger<GeneralObjectCacheKeysGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralObjectCacheKeysGenerator"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public GeneralObjectCacheKeysGenerator(ILogger<GeneralObjectCacheKeysGenerator> logger) => this.logger = logger;

    /// <summary>
    /// Generates dummy keys for a given general object item.
    /// </summary>
    /// <param name="generalInfo">Generalized object info.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(GeneralizedInfo generalInfo)
    {
        if (generalInfo is null)
        {
            logger.LogError("Failed to generate dummy keys for general object item. '{paramName}' was null.", nameof(generalInfo));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>()
        {
            // All objects
           CacheHelper.BuildCacheItemName(new[] { generalInfo.TypeInfo.ObjectClassName, "all" }),

           // By id
           CacheHelper.BuildCacheItemName(new[] { generalInfo.TypeInfo.ObjectClassName, "byid", generalInfo.ObjectID.ToString() }),
        };

        if (!string.IsNullOrEmpty(generalInfo.ObjectCodeName))
        {
            // By object code name
            set.Add(CacheHelper.BuildCacheItemName(new[] { generalInfo.TypeInfo.ObjectClassName, "byname", generalInfo.ObjectCodeName }));
        }

        if (generalInfo.ObjectGUID != Guid.Empty)
        {
            // By object guid
            set.Add(CacheHelper.BuildCacheItemName(new[] { generalInfo.TypeInfo.ObjectClassName, "byguid", generalInfo.ObjectGUID.ToString() }));
        }

        return set;
    }
}
