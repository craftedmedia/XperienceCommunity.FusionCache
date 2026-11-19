using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.FusionCache.Admin;

[assembly: UIApplication(RedisCacheApplication.Identifier, typeof(RedisCacheApplication), "fusion-cache",
    "Fusion Cache", BaseApplicationCategories.DEVELOPMENT, Icons.Database, TemplateNames.SECTION_LAYOUT)]

namespace XperienceCommunity.FusionCache.Admin;

[UIPermission(SystemPermissions.VIEW)]
[UIPermission(PurgePermission, "Purge caches")]
public sealed class RedisCacheApplication : ApplicationPage
{
    public const string Identifier = "XperienceCommunity.FusionCache.Redis";
    public const string PurgePermission = "XperienceCommunity.FusionCache.Purge";
}
