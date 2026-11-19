using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;

using XperienceCommunity.FusionCache.Admin;
using XperienceCommunity.FusionCache.Management;

[assembly: UIPage(typeof(RedisCacheApplication), "purge-all", typeof(FusionCachePurgePage),
    "Purge All", TemplateNames.EDIT, 20)]

namespace XperienceCommunity.FusionCache.Admin;

[UIEvaluatePermission(RedisCacheApplication.PurgePermission)]
public sealed class FusionCachePurgePage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IRedisCacheManagementService cacheManagement,
    IAuthenticatedUserAccessor userAccessor,
    IEventLogService eventLog)
    : RedisCachePurgePage(formItemCollectionProvider, formDataBinder, cacheManagement, userAccessor, eventLog)
{
    protected override bool PurgeAllCacheLayers => true;

    protected override async Task<ICollection<IFormItem>> GetFormItems() =>
        (await base.GetFormItems()).Where(item =>
            item is not IFormComponent component || component.Name == nameof(RedisCachePurgeModel.Confirmation)).ToList();
}
