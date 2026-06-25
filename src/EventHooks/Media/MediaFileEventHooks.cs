using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.MediaLibrary;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.EventHooks.Media.MediaFileEventHooks))]

namespace XperienceCommunity.FusionCache.EventHooks.Media;

/// <summary>
/// Registers cache invalidation handlers for media file changes.
/// </summary>
internal sealed class MediaFileEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileEventHooks"/> class.
    /// </summary>
    public MediaFileEventHooks()
        : base(nameof(MediaFileEventHooks))
    {
    }

    /// <summary>
    /// Registers media file object event handlers.
    /// </summary>
    /// <param name="parameters">Module pre-initialization parameters.</param>
    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterInsertEvent<MediaFileInfo>, MediaFileAfterInsertEventHandler>();
        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterUpdateEvent<MediaFileInfo>, MediaFileAfterUpdateEventHandler>();
        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterDeleteEvent<MediaFileInfo>, MediaFileAfterDeleteEventHandler>();
    }
}
