using System.Reflection;

using CMS;
using CMS.Core;
using CMS.DataEngine;

using CommonServiceLocator;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Providers;
using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.Caching.Utilities;
using XperienceCommunity.FusionCache.Utilities;

[assembly: RegisterModule(typeof(XperienceCommunity.FusionCache.Caching.EventHooks.GeneralObjectEventHooks))]

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Handles cache clearing events for general objects.
/// </summary>
internal class GeneralObjectEventHooks : CMS.DataEngine.Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralObjectEventHooks"/> class.
    /// </summary>
    public GeneralObjectEventHooks()
        : base(nameof(GeneralObjectEventHooks))
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
        try
        {
            var types = TypeFinder.GetImplementationsOf<IGeneralObjectCacheItemsProvider>(Assembly.GetEntryAssembly());

            if (types is not null && types.Any())
            {
                foreach (var type in types)
                {
                    if (type is null)
                    {
                        continue;
                    }

                    var provider = Activator.CreateInstance(type) as IGeneralObjectCacheItemsProvider;

                    if (provider?.GeneralObjectInfos is null)
                    {
                        continue;
                    }

                    foreach (var objectType in provider.GeneralObjectInfos)
                    {
                        objectType.Events.Insert.After += GeneralObject_Change;
                        objectType.Events.Update.After += GeneralObject_Change;
                        objectType.Events.Delete.After += GeneralObject_Change;
                    }
                }
            }
        }
        catch (Exception exc)
        {
            Service.Resolve<IEventLogService>().LogException(nameof(GeneralObjectEventHooks), nameof(AddEventHooks), exc, additionalMessage: "An unexpected error occurred whilst setting up general object info CMS hooks.");
        }
    }

    private void GeneralObject_Change(object? sender, ObjectEventArgs e)
    {
        try
        {
            if (e.Object is BaseInfo baseInfo)
            {
                var cacheKeyGenerator = ServiceLocator.Current.GetInstance<GeneralObjectCacheKeysGenerator>();
                var dummyCacheKeysService = ServiceLocator.Current.GetInstance<DummyCacheKeysService>();
                var cacheKeys = cacheKeyGenerator.GetDummyKeys(baseInfo);

                dummyCacheKeysService.TouchDummyKeys(cacheKeys);
            }
        }
        catch (Exception exc)
        {
            Service.Resolve<IEventLogService>().LogException(nameof(GeneralObjectEventHooks), nameof(GeneralObject_Change), exc, additionalMessage: $"An unexpected error occurred whilst executing the '{nameof(GeneralObject_Change)}' hook.");
        }
    }
}
