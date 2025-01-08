using CMS.Websites;

namespace XperienceCommunity.FusionCache.Caching.EventHooks;

/// <summary>
/// Generalized web page event args.
/// </summary>
internal class WebPageEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageEventArgs"/> class.
    /// </summary>
    /// <param name="args">Instance of <see cref="WebPageEventArgsBase"/>.</param>
    public WebPageEventArgs(WebPageEventArgsBase args)
    {
        ID = args.ID;
        ParentID = args.ParentID;
        Guid = args.Guid;
        Name = args.Name;
        TreePath = args.TreePath;
        Order = args.Order;
        WebsiteChannelID = args.WebsiteChannelID;
        WebsiteChannelName = args.WebsiteChannelName;
        ContentTypeID = args.ContentTypeID;
        ContentTypeName = args.ContentTypeName;
        ContentLanguageID = args.ContentLanguageID;
        ContentLanguageName = args.ContentLanguageName;
        IsSecured = args.IsSecured;
        DisplayName = args.DisplayName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPageEventArgs"/> class.
    /// </summary>
    /// <param name="args">Instance of <see cref="CreateWebPageEventArgs"/>.</param>
    public WebPageEventArgs(CreateWebPageEventArgs args)
    {
        ID = args.ID ?? -1;
        ParentID = args.ParentID;
        Guid = args.Guid ?? Guid.Empty;
        Name = args.Name;
        TreePath = args.TreePath;
        Order = args.Order;
        WebsiteChannelID = args.WebsiteChannelID;
        WebsiteChannelName = args.WebsiteChannelName;
        ContentTypeID = args.ContentTypeID;
        ContentTypeName = args.ContentTypeName;
        ContentLanguageID = args.ContentLanguageID;
        ContentLanguageName = args.ContentLanguageName;
        IsSecured = args.IsSecured;
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
