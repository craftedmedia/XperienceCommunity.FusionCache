using CMS;
using CMS.Core;
using CMS.DataEngine;

using CommonServiceLocator;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.Utilities;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.Caching.EventHooks.SettingsKeyEventHooks))]

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Handles cache clearing events for settings key items.
/// </summary>
internal class SettingsKeyEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsKeyEventHooks"/> class.
    /// </summary>
    public SettingsKeyEventHooks()
        : base(nameof(SettingsKeyEventHooks))
    {
    }

    /// <summary>
    /// Contains initialization code that is executed when the application starts.
    /// </summary>
    protected override void OnInit()
    {
        base.OnInit();

        if (XperienceAdminHelper.IsRunningAdmin())
        {
            AddEventHooks();
        }
    }

    private void AddEventHooks() => SettingsKeyInfo.TYPEINFO.Events.Update.After += SettingsKey_Change;

    private void SettingsKey_Change(object? sender, ObjectEventArgs e)
    {
        try
        {
            if (e.Object is SettingsKeyInfo settingsKeyInfo)
            {
                var cacheKeyGenerator = ServiceLocator.Current.GetInstance<SettingsKeyCacheKeyGenerator>();
                var dummyCacheKeysService = ServiceLocator.Current.GetInstance<DummyCacheKeysService>();
                var cacheKeys = cacheKeyGenerator.GetDummyKeys(settingsKeyInfo.KeyName);

                dummyCacheKeysService.TouchDummyKeys(cacheKeys);
            }
        }
        catch (Exception exc)
        {
            Service.Resolve<IEventLogService>().LogException(nameof(SettingsKeyEventHooks), nameof(SettingsKey_Change), exc, additionalMessage: $"An unexpected error occurred whilst executing the '{nameof(SettingsKey_Change)}' hook.");
        }
    }
}
