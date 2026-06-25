using CMS.DataEngine;

namespace XperienceCommunity.FusionCache.EventHooks.General
{
    /// <summary>
    /// Handles general object update events.
    /// </summary>
    internal sealed class GeneralObjectAfterUpdateEventHandler : IInfoObjectEventHandler<InfoObjectAfterUpdateEvent>
    {
        private readonly GeneralObjectCacheInvalidator invalidator;

        public GeneralObjectAfterUpdateEventHandler(GeneralObjectCacheInvalidator invalidator) =>
            this.invalidator = invalidator;

        public void Handle(InfoObjectAfterUpdateEvent infoObjectEvent) =>
            invalidator.Invalidate(
                infoObjectEvent.InfoObject,
                nameof(InfoObjectAfterUpdateEvent));

        public Task HandleAsync(
            InfoObjectAfterUpdateEvent infoObjectEvent,
            CancellationToken cancellationToken) =>
            invalidator.InvalidateAsync(
                infoObjectEvent.InfoObject,
                nameof(InfoObjectAfterUpdateEvent),
                cancellationToken);
    }
}
