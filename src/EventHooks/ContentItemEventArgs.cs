using CMS.ContentEngine;

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Generalized content item event args.
/// </summary>
internal class ContentItemEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentItemEventArgs"/> class.
    /// </summary>
    /// <param name="args">Instance of <see cref="ContentItemEventArgsBase"/>.</param>
    public ContentItemEventArgs(ContentItemEventArgsBase args)
    {
        ID = args.ID;
        Name = args.Name;
        Guid = args.Guid;
        IsSecured = args.IsSecured;
        ContentTypeID = args.ContentTypeID;
        ContentTypeName = args.ContentTypeName;
        ContentLanguageID = args.ContentLanguageID;
        ContentLanguageName = args.ContentLanguageName;
        DisplayName = args.DisplayName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentItemEventArgs"/> class.
    /// </summary>
    /// <param name="args">Instance of <see cref="CreateContentItemEventArgs"/>.</param>
    public ContentItemEventArgs(CreateContentItemEventArgs args)
    {
        ID = args.ID ?? -1;
        Name = args.Name;
        Guid = args.GUID ?? Guid.Empty;
        IsSecured = args.IsSecured;
        ContentTypeID = args.ContentTypeID;
        ContentTypeName = args.ContentTypeName;
        ContentLanguageID = args.ContentLanguageID;
        ContentLanguageName = args.ContentLanguageName;
        DisplayName = args.DisplayName;
    }

    /// <summary>
    /// Gets the web page ID.
    /// </summary>
    public int ID { get; }

    /// <summary>
    /// Gets the web page parent ID.
    /// </summary>
    public int ParentID { get; }

    /// <summary>
    /// Gets the web page item GUID.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the web page item tree path.
    /// </summary>
    public string TreePath { get; } = string.Empty;

    /// <summary>
    /// Gets the web page item order.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the website channel ID.
    /// </summary>
    public int WebsiteChannelID { get; }

    /// <summary>
    /// Gets the website channel name.
    /// </summary>
    public string WebsiteChannelName { get; } = string.Empty;

    /// <summary>
    /// Gets the content type ID.
    /// </summary>
    public int ContentTypeID { get; }

    /// <summary>
    /// Gets the content type name.
    /// </summary>
    public string ContentTypeName { get; } = string.Empty;

    /// <summary>
    /// Gets the content language ID.
    /// </summary>
    public int ContentLanguageID { get; }

    /// <summary>
    /// Gets the content language name.
    /// </summary>
    public string ContentLanguageName { get; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the web page item is secured.
    /// </summary>
    public bool IsSecured { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; } = string.Empty;
}
