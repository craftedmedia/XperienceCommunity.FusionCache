using CMS.DataEngine;

namespace XperienceCommunity.FusionCache.EventHooks.General
{
    /// <summary>
    /// Handles general object insert events.
    /// </summary>
    internal sealed class GeneralObjectAfterInsertEventHandler : IInfoObjectEventHandler<InfoObjectAfterInsertEvent>
    {
        private readonly GeneralObjectCacheInvalidator invalidator;

        public GeneralObjectAfterInsertEventHandler(GeneralObjectCacheInvalidator invalidator) =>
            this.invalidator = invalidator;

        public void Handle(InfoObjectAfterInsertEvent infoObjectEvent) =>
            invalidator.Invalidate(
                infoObjectEvent.InfoObject,
                nameof(InfoObjectAfterInsertEvent));

        public Task HandleAsync(
            InfoObjectAfterInsertEvent infoObjectEvent,
            CancellationToken cancellationToken) =>
            invalidator.InvalidateAsync(
                infoObjectEvent.InfoObject,
                nameof(InfoObjectAfterInsertEvent),
                cancellationToken);
    }
}
