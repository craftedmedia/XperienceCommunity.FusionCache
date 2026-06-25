using CMS;
using CMS.Core;
using CMS.DataEngine;

using XperienceCommunity.FusionCache.EventHooks.General;

[assembly: RegisterModule(typeof(GeneralObjectEventHooks))]

namespace XperienceCommunity.FusionCache.EventHooks.General;

/// <summary>
/// Handles cache clearing events for general objects.
/// </summary>
internal class GeneralObjectEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralObjectEventHooks"/> class.
    /// </summary>
    public GeneralObjectEventHooks()
        : base(nameof(GeneralObjectEventHooks))
    {
    }

    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterInsertEvent, GeneralObjectAfterInsertEventHandler>();
        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterUpdateEvent, GeneralObjectAfterUpdateEventHandler>();
        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterDeleteEvent, GeneralObjectAfterDeleteEventHandler>();
    }
}
