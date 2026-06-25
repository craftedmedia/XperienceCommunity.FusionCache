using CMS.DataEngine;

namespace XperienceCommunity.FusionCache.EventHooks.General
{
    /// <summary>
    /// Handles general object delete events.
    /// </summary>
    internal sealed class GeneralObjectAfterDeleteEventHandler : IInfoObjectEventHandler<InfoObjectAfterDeleteEvent>
    {
        private readonly GeneralObjectCacheInvalidator invalidator;

        public GeneralObjectAfterDeleteEventHandler(GeneralObjectCacheInvalidator invalidator) =>
            this.invalidator = invalidator;

        public void Handle(InfoObjectAfterDeleteEvent infoObjectEvent) =>
            invalidator.Invalidate(
                infoObjectEvent.InfoObject,
                nameof(InfoObjectAfterDeleteEvent));

        public Task HandleAsync(
            InfoObjectAfterDeleteEvent infoObjectEvent,
            CancellationToken cancellationToken) =>
            invalidator.InvalidateAsync(
                infoObjectEvent.InfoObject,
                nameof(InfoObjectAfterDeleteEvent),
                cancellationToken);
    }
}
