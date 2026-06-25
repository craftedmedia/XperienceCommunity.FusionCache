using CMS.Core;

using Microsoft.Extensions.Options;

using XperienceCommunity.FusionCache.Caching.Services;
using XperienceCommunity.FusionCache.KeyGenerators;
using XperienceCommunity.FusionCache.Utilities;

namespace XperienceCommunity.FusionCache.EventHooks;

/// <summary>
/// Base cache invalidator for event handlers that invalidate FusionCache entries
/// using Kentico-style dummy cache keys.
/// </summary>
/// <typeparam name="TCacheKeysGenerator">The cache key generator type.</typeparam>
/// <typeparam name="TEventArgs">The event argument type used by the cache key generator.</typeparam>
internal abstract class EventCacheInvalidator<TCacheKeysGenerator, TEventArgs>
    where TCacheKeysGenerator : ICacheKeysGenerator<TEventArgs>
{
    private readonly TCacheKeysGenerator cacheKeyGenerator;
    private readonly DummyCacheKeysService dummyCacheKeysService;
    private readonly IEventLogService eventLogService;
    private readonly bool canInvalidateOnCurrentInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventCacheInvalidator{TCacheKeysGenerator, TEventArgs}"/> class.
    /// </summary>
    /// <param name="cacheKeyGenerator">Instance of <typeparamref name="TCacheKeysGenerator"/>.</param>
    /// <param name="dummyCacheKeysService">Instance of <see cref="DummyCacheKeysService"/>.</param>
    /// <param name="eventLogService">Instance of <see cref="IEventLogService"/>.</param>
    /// <param name="options">Instance of <see cref="IOptions{XperienceCommunityFusionCacheOptions}"/>.</param>
    protected EventCacheInvalidator(
        TCacheKeysGenerator cacheKeyGenerator,
        DummyCacheKeysService dummyCacheKeysService,
        IEventLogService eventLogService,
        IOptions<XperienceCommunityFusionCacheOptions> options)
    {
        this.cacheKeyGenerator = cacheKeyGenerator;
        this.dummyCacheKeysService = dummyCacheKeysService;
        this.eventLogService = eventLogService;

        canInvalidateOnCurrentInstance =
            !RegisterEventHandlersOnlyInAdmin(options.Value) ||
            XperienceAdminHelper.IsRunningAdmin();
    }

    public void Invalidate(TEventArgs? eventArgs, string eventName)
    {
        if (!canInvalidateOnCurrentInstance)
        {
            return;
        }

        try
        {
            if (eventArgs is null || !ShouldInvalidate(eventArgs))
            {
                return;
            }

            var cacheKeys = cacheKeyGenerator.GetDummyKeys(eventArgs);

            dummyCacheKeysService.TouchDummyKeys(cacheKeys);
        }
        catch (Exception exc)
        {
            eventLogService.LogException(
                Source,
                eventName,
                exc,
                additionalMessage: $"An unexpected error occurred while invalidating FusionCache entries for the '{eventName}' event.");
        }
    }

    public async Task InvalidateAsync(
        TEventArgs? eventArgs,
        string eventName,
        CancellationToken cancellationToken)
    {
        if (!canInvalidateOnCurrentInstance)
        {
            return;
        }

        try
        {
            if (eventArgs is null || !ShouldInvalidate(eventArgs))
            {
                return;
            }

            var cacheKeys = cacheKeyGenerator.GetDummyKeys(eventArgs);

            await dummyCacheKeysService.TouchDummyKeysAsync(cacheKeys, cancellationToken);
        }
        catch (Exception exc)
        {
            eventLogService.LogException(
                Source,
                eventName,
                exc,
                additionalMessage: $"An unexpected error occurred while invalidating FusionCache entries for the '{eventName}' event.");
        }
    }

    /// <summary>
    /// Gets the source used when logging event log entries.
    /// </summary>
    protected virtual string Source => GetType().Name;

    /// <summary>
    /// Determines whether this invalidator should be admin-only.
    /// </summary>
    protected virtual bool RegisterEventHandlersOnlyInAdmin(XperienceCommunityFusionCacheOptions options) => options.RegisterEventHandlersOnlyInAdmin;

    /// <summary>
    /// Allows derived invalidators to skip irrelevant events.
    /// </summary>
    protected virtual bool ShouldInvalidate(TEventArgs eventArgs) => true;
}
