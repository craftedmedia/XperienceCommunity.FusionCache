using CMS.DataEngine;
using CMS.MediaLibrary;

namespace XperienceCommunity.FusionCache.EventHooks.Media;

/// <summary>
/// Handles media file delete events.
/// </summary>
internal sealed class MediaFileAfterDeleteEventHandler
    : IInfoObjectEventHandler<InfoObjectAfterDeleteEvent<MediaFileInfo>>
{
    private readonly MediaFileCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileAfterDeleteEventHandler"/> class.
    /// </summary>
    public MediaFileAfterDeleteEventHandler(MediaFileCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public void Handle(InfoObjectAfterDeleteEvent<MediaFileInfo> infoObjectEvent) =>
        invalidator.Invalidate(
            infoObjectEvent.InfoObject.FileGUID,
            nameof(InfoObjectAfterDeleteEvent<MediaFileInfo>));

    /// <inheritdoc />
    public Task HandleAsync(
        InfoObjectAfterDeleteEvent<MediaFileInfo> infoObjectEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            infoObjectEvent.InfoObject.FileGUID,
            nameof(InfoObjectAfterDeleteEvent<MediaFileInfo>),
            cancellationToken);
}
