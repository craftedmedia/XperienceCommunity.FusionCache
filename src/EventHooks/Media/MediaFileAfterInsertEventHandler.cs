using CMS.DataEngine;
using CMS.MediaLibrary;

namespace XperienceCommunity.FusionCache.EventHooks.Media;

/// <summary>
/// Handles media file insert events.
/// </summary>
internal sealed class MediaFileAfterInsertEventHandler
    : IInfoObjectEventHandler<InfoObjectAfterInsertEvent<MediaFileInfo>>
{
    private readonly MediaFileCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFileAfterInsertEventHandler"/> class.
    /// </summary>
    public MediaFileAfterInsertEventHandler(MediaFileCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public void Handle(InfoObjectAfterInsertEvent<MediaFileInfo> infoObjectEvent) =>
        invalidator.Invalidate(
            infoObjectEvent.InfoObject.FileGUID,
            nameof(InfoObjectAfterInsertEvent<MediaFileInfo>));

    /// <inheritdoc />
    public Task HandleAsync(
        InfoObjectAfterInsertEvent<MediaFileInfo> infoObjectEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            infoObjectEvent.InfoObject.FileGUID,
            nameof(InfoObjectAfterInsertEvent<MediaFileInfo>),
            cancellationToken);
}
