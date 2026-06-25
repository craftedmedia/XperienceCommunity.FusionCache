using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.EventHooks.WebPage.WebPageEventHooks))]

namespace XperienceCommunity.FusionCache.EventHooks.WebPage;

/// <summary>
/// Registers cache invalidation handlers for web page changes.
/// </summary>
internal sealed class WebPageEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageEventHooks"/> class.
    /// </summary>
    public WebPageEventHooks()
        : base(nameof(WebPageEventHooks))
    {
    }

    /// <summary>
    /// Registers web page event handlers.
    /// </summary>
    /// <param name="parameters">Module pre-initialization parameters.</param>
    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        parameters.Services.AddEventHandler<AfterCreateWebPageEvent, WebPageAfterCreateEventHandler>();
        parameters.Services.AddEventHandler<AfterPublishWebPageEvent, WebPageAfterPublishEventHandler>();
        parameters.Services.AddEventHandler<AfterDeleteWebPageEvent, WebPageAfterDeleteEventHandler>();
    }
}
