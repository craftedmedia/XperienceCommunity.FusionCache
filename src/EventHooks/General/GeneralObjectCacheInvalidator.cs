using CMS.Core;
using CMS.DataEngine;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.KeyGenerators;
using XperienceCommunity.FusionCache.Caching.Providers;
using XperienceCommunity.FusionCache.Caching.Services;

namespace XperienceCommunity.FusionCache.EventHooks.General
{
    /// <summary>
    /// Invalidates FusionCache entries for configured general object events.
    /// </summary>
    internal sealed class GeneralObjectCacheInvalidator
        : EventCacheInvalidator<GeneralObjectCacheKeysGenerator, GeneralizedInfo>
    {
        private readonly IEventLogService eventLogService;
        private readonly IReadOnlyCollection<IGeneralObjectCacheItemsProvider> providers;
        private readonly Lazy<HashSet<string>> trackedObjectClassNames;

        public GeneralObjectCacheInvalidator(
            GeneralObjectCacheKeysGenerator cacheKeyGenerator,
            DummyCacheKeysService dummyCacheKeysService,
            IEventLogService eventLogService,
            IEnumerable<IGeneralObjectCacheItemsProvider> providers,
            IOptions<XperienceCommunityFusionCacheOptions> options)
            : base(cacheKeyGenerator, dummyCacheKeysService, eventLogService, options)
        {
            this.eventLogService = eventLogService;
            this.providers = providers.ToArray();

            trackedObjectClassNames = new Lazy<HashSet<string>>(BuildTrackedObjectClassNames);
        }

        protected override string Source => nameof(GeneralObjectCacheInvalidator);

        protected override bool ShouldInvalidate(GeneralizedInfo generalInfo)
        {
            string? objectClassName = generalInfo.TypeInfo?.ObjectClassName;

            return !string.IsNullOrWhiteSpace(objectClassName) &&
                   trackedObjectClassNames.Value.Contains(objectClassName);
        }

        private HashSet<string> BuildTrackedObjectClassNames()
        {
            var objectClassNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                try
                {
                    if (provider.GeneralObjectInfos is null)
                    {
                        continue;
                    }

                    foreach (var objectTypeInfo in provider.GeneralObjectInfos)
                    {
                        if (!string.IsNullOrWhiteSpace(objectTypeInfo?.ObjectClassName))
                        {
                            objectClassNames.Add(objectTypeInfo.ObjectClassName);
                        }
                    }
                }
                catch (Exception exc)
                {
                    eventLogService.LogException(
                        nameof(GeneralObjectCacheInvalidator),
                        nameof(BuildTrackedObjectClassNames),
                        exc,
                        additionalMessage: $"An unexpected error occurred while reading general object cache provider '{provider.GetType().FullName}'.");
                }
            }

            return objectClassNames;
        }
    }
}
