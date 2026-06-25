using CMS.Core;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;

namespace XperienceCommunity.FusionCache.EventHooks.Settings;

/// <summary>
/// Invalidates FusionCache entries for settings key events.
/// </summary>
internal sealed class SettingsKeyCacheInvalidator
    : EventCacheInvalidator<SettingsKeyCacheKeyGenerator, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsKeyCacheInvalidator"/> class.
    /// </summary>
    public SettingsKeyCacheInvalidator(
        SettingsKeyCacheKeyGenerator cacheKeyGenerator,
        DummyCacheKeysService dummyCacheKeysService,
        IEventLogService eventLogService,
        IOptions<XperienceCommunityFusionCacheOptions> options)
        : base(cacheKeyGenerator, dummyCacheKeysService, eventLogService, options)
    {
    }

    /// <inheritdoc />
    protected override string Source => nameof(SettingsKeyEventHooks);

    /// <inheritdoc />
    protected override bool ShouldInvalidate(string keyName) =>
        !string.IsNullOrWhiteSpace(keyName);
}
