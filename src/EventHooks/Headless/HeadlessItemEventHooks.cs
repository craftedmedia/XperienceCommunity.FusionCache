using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Headless;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.EventHooks.Headless.HeadlessItemEventHooks))]

namespace XperienceCommunity.FusionCache.EventHooks.Headless;

/// <summary>
/// Registers cache invalidation handlers for headless item changes.
/// </summary>
internal sealed class HeadlessItemEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemEventHooks"/> class.
    /// </summary>
    public HeadlessItemEventHooks()
        : base(nameof(HeadlessItemEventHooks))
    {
    }

    /// <summary>
    /// Registers headless item event handlers.
    /// </summary>
    /// <param name="parameters">Module pre-initialization parameters.</param>
    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        parameters.Services.AddEventHandler<AfterCreateHeadlessItemEvent, HeadlessItemAfterCreateEventHandler>();
        parameters.Services.AddEventHandler<AfterPublishHeadlessItemEvent, HeadlessItemAfterPublishEventHandler>();
        parameters.Services.AddEventHandler<AfterDeleteHeadlessItemEvent, HeadlessItemAfterDeleteEventHandler>();
    }
}
