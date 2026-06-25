using CMS;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.EventHooks.Settings.SettingsKeyEventHooks))]

namespace XperienceCommunity.FusionCache.EventHooks.Settings;

/// <summary>
/// Registers cache invalidation handlers for settings key changes.
/// </summary>
internal sealed class SettingsKeyEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsKeyEventHooks"/> class.
    /// </summary>
    public SettingsKeyEventHooks()
        : base(nameof(SettingsKeyEventHooks))
    {
    }

    /// <summary>
    /// Registers settings key object event handlers.
    /// </summary>
    /// <param name="parameters">Module pre-initialization parameters.</param>
    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterUpdateEvent<SettingsKeyInfo>, SettingsKeyAfterUpdateEventHandler>();
    }
}
