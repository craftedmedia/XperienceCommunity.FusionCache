using CMS.DataEngine;

namespace XperienceCommunity.FusionCache.EventHooks.Settings;

/// <summary>
/// Handles settings key update events.
/// </summary>
internal sealed class SettingsKeyAfterUpdateEventHandler
    : IInfoObjectEventHandler<InfoObjectAfterUpdateEvent<SettingsKeyInfo>>
{
    private readonly SettingsKeyCacheInvalidator invalidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsKeyAfterUpdateEventHandler"/> class.
    /// </summary>
    public SettingsKeyAfterUpdateEventHandler(SettingsKeyCacheInvalidator invalidator) =>
        this.invalidator = invalidator;

    /// <inheritdoc />
    public void Handle(InfoObjectAfterUpdateEvent<SettingsKeyInfo> infoObjectEvent) =>
        invalidator.Invalidate(
            infoObjectEvent.InfoObject.KeyName,
            nameof(InfoObjectAfterUpdateEvent<SettingsKeyInfo>));

    /// <inheritdoc />
    public Task HandleAsync(
        InfoObjectAfterUpdateEvent<SettingsKeyInfo> infoObjectEvent,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateAsync(
            infoObjectEvent.InfoObject.KeyName,
            nameof(InfoObjectAfterUpdateEvent<SettingsKeyInfo>),
            cancellationToken);
}
