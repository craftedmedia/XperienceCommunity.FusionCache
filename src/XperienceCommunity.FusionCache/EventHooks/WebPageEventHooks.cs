using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;

using CommonServiceLocator;

using Newtonsoft.Json;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.Utilities;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.Caching.EventHooks.WebPageEventHooks))]

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Handles cache clearing events for web page items.
/// </summary>
internal class WebPageEventHooks : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageEventHooks"/> class.
    /// </summary>
    public WebPageEventHooks()
        : base(nameof(WebPageEventHooks))
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
        WebPageEvents.Publish.Execute += WebPageItem_Publish;
        WebPageEvents.Delete.Execute += WebPageItem_Delete;
        WebPageEvents.Create.After += WebPageItem_Create;
    }

    private void WebPageItem_Create(object? sender, CreateWebPageEventArgs e) => WebPageItem_Change(new WebPageEventArgs(e));

    private void WebPageItem_Delete(object? sender, DeleteWebPageEventArgs e) => WebPageItem_Change(new WebPageEventArgs(e));

    private void WebPageItem_Publish(object? sender, PublishWebPageEventArgs e) => WebPageItem_Change(new WebPageEventArgs(e));

    private void WebPageItem_Change(WebPageEventArgs e)
    {
        try
        {
            var cacheKeyGenerator = ServiceLocator.Current.GetInstance<WebPageCacheKeysGenerator>();
            var dummyCacheKeysService = ServiceLocator.Current.GetInstance<DummyCacheKeysService>();
            var cacheKeys = cacheKeyGenerator.GetDummyKeys(e);

            dummyCacheKeysService.TouchDummyKeys(cacheKeys);
        }
        catch (Exception exc)
        {
            Service.Resolve<IEventLogService>().LogException(nameof(WebPageEventHooks), nameof(WebPageItem_Change), exc, additionalMessage: $"An unexpected error occurred whilst executing the '{nameof(WebPageItem_Change)}' hook.");
        }
    }
}
