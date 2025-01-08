using CMS.DataEngine;

namespace XperienceCommunity.FusionCache.Caching.Providers;

/// <summary>
/// Provides a way to extend the cache clearing function so that it can monitor custom object types.
/// </summary>
public interface IGeneralObjectCacheItemsProvider
{
    /// <summary>
    /// Gets a collection of <see cref="ObjectTypeInfo"/> objects representing general objects which should react to CMS events, clearing any associated cache entries.
    /// </summary>
    /// <returns>Collection of <see cref="ObjectTypeInfo"/> objects to monitor for changes.</returns>
    IEnumerable<ObjectTypeInfo> GeneralObjectInfos { get; }
}
