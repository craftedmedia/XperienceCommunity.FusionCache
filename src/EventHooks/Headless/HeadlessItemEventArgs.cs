using CMS.Headless;

namespace XperienceCommunity.FusionCache.EventHooks.Headless;

/// <summary>
/// Normalized event data used by headless item cache key generation.
/// </summary>
internal sealed class HeadlessItemEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemEventArgs"/> class.
    /// </summary>
    /// <param name="data">Instance of <see cref="CreateHeadlessItemEventData"/>.</param>
    public HeadlessItemEventArgs(CreateHeadlessItemEventData data)
        : this(
            data.ID.GetValueOrDefault(),
            data.Guid.GetValueOrDefault(),
            data.Name,
            data.DisplayName,
            data.ContentLanguageID,
            data.ContentLanguageName,
            data.ContentTypeID,
            data.ContentTypeName,
            data.HeadlessChannelID,
            data.HeadlessChannelName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeadlessItemEventArgs"/> class.
    /// </summary>
    /// <param name="data">Instance of <see cref="IHeadlessItemEventArgs"/>.</param>
    public HeadlessItemEventArgs(IHeadlessItemEventArgs data)
        : this(
            data.ID,
            data.Guid,
            data.Name,
            data.DisplayName,
            data.ContentLanguageID,
            data.ContentLanguageName,
            data.ContentTypeID,
            data.ContentTypeName,
            data.HeadlessChannelID,
            data.HeadlessChannelName)
    {
    }

    private HeadlessItemEventArgs(
        int id,
        Guid guid,
        string name,
        string displayName,
        int contentLanguageId,
        string contentLanguageName,
        int contentTypeId,
        string contentTypeName,
        int headlessChannelId,
        string headlessChannelName)
    {
        ID = id;
        Guid = guid;
        Name = name;
        DisplayName = displayName;
        ContentLanguageID = contentLanguageId;
        ContentLanguageName = contentLanguageName;
        ContentTypeID = contentTypeId;
        ContentTypeName = contentTypeName;
        HeadlessChannelID = headlessChannelId;
        HeadlessChannelName = headlessChannelName;
    }

    /// <summary>
    /// Gets the headless item identifier.
    /// </summary>
    public int ID { get; }

    /// <summary>
    /// Gets the headless item GUID.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets the headless item code name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the headless item display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the identifier of the content language assigned to the headless item.
    /// </summary>
    public int ContentLanguageID { get; }

    /// <summary>
    /// Gets the code name of the content language assigned to the headless item.
    /// </summary>
    public string ContentLanguageName { get; }

    /// <summary>
    /// Gets the identifier of the content type assigned to the headless item.
    /// </summary>
    public int ContentTypeID { get; }

    /// <summary>
    /// Gets the code name of the content type assigned to the headless item.
    /// </summary>
    public string ContentTypeName { get; }

    /// <summary>
    /// Gets the identifier of the headless channel that contains the headless item.
    /// </summary>
    public int HeadlessChannelID { get; }

    /// <summary>
    /// Gets the code name of the headless channel that contains the headless item.
    /// </summary>
    public string HeadlessChannelName { get; }
}
