using CMS;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;

using XperienceCommunity.FusionCache.EventHooks.ContentItem;

[assembly: RegisterModule(typeof(ContentItemEventHooks))]

namespace XperienceCommunity.FusionCache.EventHooks.ContentItem;

/// <summary>
/// Registers cache invalidation handlers for content item changes.
/// </summary>
internal sealed class ContentItemEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentItemEventHooks"/> class.
    /// </summary>
    public ContentItemEventHooks()
        : base(nameof(ContentItemEventHooks))
    {
    }

    /// <summary>
    /// Registers content item event handlers.
    /// </summary>
    /// <param name="parameters">Module pre-initialization parameters.</param>
    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        parameters.Services.AddEventHandler<AfterCreateContentItemEvent, ContentItemAfterCreateEventHandler>();
        parameters.Services.AddEventHandler<AfterPublishContentItemEvent, ContentItemAfterPublishEventHandler>();
        parameters.Services.AddEventHandler<AfterDeleteContentItemEvent, ContentItemAfterDeleteEventHandler>();
    }
}
