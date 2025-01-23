using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.MediaLibrary;

using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.Utilities;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.Caching.EventHooks.MediaFileEventHooks))]

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Handles cache clearing events for media file items.
/// </summary>
internal class MediaFileEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileEventHooks"/> class.
    /// </summary>
    public MediaFileEventHooks()
        : base(nameof(MediaFileEventHooks))
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

    private void AddEventHooks()
    {
        MediaFileInfo.TYPEINFO.Events.Insert.After += MediaFile_Change;
        MediaFileInfo.TYPEINFO.Events.Update.After += MediaFile_Change;
        MediaFileInfo.TYPEINFO.Events.Delete.After += MediaFile_Change;
    }

    private void MediaFile_Change(object? sender, ObjectEventArgs e)
    {
        try
        {
            if (e.Object is MediaFileInfo mediaFileInfo)
            {
                var cacheKeyGenerator = ServiceContainer.Instance.GetService<MediaFileCacheKeysGenerator>();
                var dummyCacheKeysService = ServiceContainer.Instance.GetService<DummyCacheKeysService>();

                if (cacheKeyGenerator is null || dummyCacheKeysService is null)
                {
                    return;
                }

                var cacheKeys = cacheKeyGenerator.GetDummyKeys(mediaFileInfo.FileGUID);
                dummyCacheKeysService.TouchDummyKeys(cacheKeys);
            }
        }
        catch (Exception exc)
        {
            Service.Resolve<IEventLogService>().LogException(nameof(MediaFileEventHooks), nameof(MediaFile_Change), exc, additionalMessage: $"An unexpected error occurred whilst executing the '{nameof(MediaFile_Change)}' hook.");
        }
    }
}
