using CMS.Helpers;

using Microsoft.Extensions.Logging;

namespace XperienceCommunity.FusionCache.Caching.KeyGenerators;

/// <summary>
/// Generates dummy cache keys for settings key items.
/// </summary>
internal class SettingsKeyCacheKeyGenerator
{
    private readonly ILogger<SettingsKeyCacheKeyGenerator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsKeyCacheKeyGenerator"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public SettingsKeyCacheKeyGenerator(ILogger<SettingsKeyCacheKeyGenerator> logger) => this.logger = logger;

    /// <summary>
    /// Generates dummy keys for a given settings item.
    /// </summary>
    /// <param name="settingsCodeName">Code name of the specified settings item.</param>
    /// <returns>Dummy cache keys.</returns>
    public IEnumerable<string> GetDummyKeys(string settingsCodeName)
    {
        if (string.IsNullOrEmpty(settingsCodeName))
        {
            logger.LogError("Failed to generate dummy keys for settings item. '{paramName}' was null.", nameof(settingsCodeName));

            return Enumerable.Empty<string>();
        }

        var set = new HashSet<string>()
        {
            // Include by code name
            CacheHelper.BuildCacheItemName(new[] { "cms.settingskey", settingsCodeName.ToString() }),
        };

        return set;
    }
}
