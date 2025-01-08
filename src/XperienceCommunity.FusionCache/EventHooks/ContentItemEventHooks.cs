using CMS;
using CMS.ContentEngine;
using CMS.Core;

using CommonServiceLocator;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.Utilities;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.Caching.EventHooks.ContentItemEventHooks))]

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Handles cache clearing events for content items.
/// </summary>
internal class ContentItemEventHooks : CMS.DataEngine.Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentItemEventHooks"/> class.
    /// </summary>
    public ContentItemEventHooks()
        : base(nameof(ContentItemEventHooks))
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
        ContentItemEvents.Publish.Execute += ContentItem_Publish;
        ContentItemEvents.Delete.Execute += ContentItem_Delete;
        ContentItemEvents.Create.After += ContentItem_Create;
    }

    private void ContentItem_Create(object? sender, CreateContentItemEventArgs e) => ContentItem_Change(new ContentItemEventArgs(e));

    private void ContentItem_Delete(object? sender, DeleteContentItemEventArgs e) => ContentItem_Change(new ContentItemEventArgs(e));

    private void ContentItem_Publish(object? sender, PublishContentItemEventArgs e) => ContentItem_Change(new ContentItemEventArgs(e));

    private void ContentItem_Change(ContentItemEventArgs e)
    {
        try
        {
            var cacheKeyGenerator = ServiceLocator.Current.GetInstance<ContentItemCacheKeysGenerator>();
            var dummyCacheKeysService = ServiceLocator.Current.GetInstance<DummyCacheKeysService>();
            var cacheKeys = cacheKeyGenerator.GetDummyKeys(e);

            dummyCacheKeysService.TouchDummyKeys(cacheKeys);
        }
        catch (Exception exc)
        {
            Service.Resolve<IEventLogService>().LogException(nameof(ContentItemEventHooks), nameof(ContentItem_Change), exc, additionalMessage: $"An unexpected error occurred whilst executing the '{nameof(ContentItem_Change)}' hook.");
        }
    }
}
