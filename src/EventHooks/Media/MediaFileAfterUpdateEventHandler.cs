using CMS.DataEngine;
using CMS.MediaLibrary;

namespace XperienceCommunity.FusionCache.EventHooks.Media;

/// <summary>
/// Handles media file update events.
/// </summary>
internal sealed class MediaFileAfterUpdateEventHandler
    : IInfoObjectEventHandler<InfoObjectAfterUpdateEvent<MediaFileInfo>>
{
    private readonly MediaFileCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileAfterUpdateEventHandler"/> class.
    /// </summary>
    public MediaFileAfterUpdateEventHandler(MediaFileCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public void Handle(InfoObjectAfterUpdateEvent<MediaFileInfo> infoObjectEvent) =>
        invalidator.Invalidate(
            infoObjectEvent.InfoObject.FileGUID,
            nameof(InfoObjectAfterUpdateEvent<MediaFileInfo>));

    /// <inheritdoc />
    public Task HandleAsync(
        InfoObjectAfterUpdateEvent<MediaFileInfo> infoObjectEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            infoObjectEvent.InfoObject.FileGUID,
            nameof(InfoObjectAfterUpdateEvent<MediaFileInfo>),
            cancellationToken);
}
