using System.Text.Encodings.Web;

using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;

using XperienceCommunity.FusionCache.Admin;
using XperienceCommunity.FusionCache.Management;

[assembly: UIPage(typeof(RedisCacheApplication), "purge", typeof(RedisCachePurgePage),
    "Purge Redis", TemplateNames.EDIT, 10)]

namespace XperienceCommunity.FusionCache.Admin;

[UIEvaluatePermission(RedisCacheApplication.PurgePermission)]
public class RedisCachePurgePage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IRedisCacheManagementService cacheManagement,
    IAuthenticatedUserAccessor userAccessor,
    IEventLogService eventLog)
    : ModelEditPage<RedisCachePurgeModel>(formItemCollectionProvider, formDataBinder)
{
    protected override RedisCachePurgeModel Model { get; } = new();
    protected virtual bool PurgeAllCacheLayers => false;

    public override async Task ConfigurePage()
    {
        await base.ConfigurePage();

        string actionLabel = PurgeAllCacheLayers ? "Purge All" : "Purge Redis";

        PageConfiguration.Headline = PurgeAllCacheLayers ? "Purge All layers (L1 + L2)" : actionLabel;
        PageConfiguration.SubmitConfiguration.Label = actionLabel;
        PageConfiguration.SubmitConfiguration.Visible = cacheManagement.IsEnabled;

        PageConfiguration.Callouts.Add(new CalloutConfiguration
        {
            Type = CalloutType.FriendlyWarning,
            ContentAsHtml = PurgeAllCacheLayers,
            Content = PurgeAllCacheLayers
                ? HtmlParagraphs(
                    "Invalidates all Fusion Cache entries in memory and Redis across applications sharing this cache's backplane.",
                    "Session and data protection keys stored outside Fusion Cache are unaffected.",
                    "Disconnected applications may miss the notification; remote completion is not acknowledged.")
                : "Deletes every key in the connected Redis database except the exclusions below, including keys used by other applications. " +
                    "Existing in-memory FusionCache entries are not cleared and may continue to be served until they expire. " +
                    "Applications can create new keys while the purge runs."
        });

        if (PurgeAllCacheLayers)
        {
            PageConfiguration.Callouts.Add(new CalloutConfiguration
            {
                Type = CalloutType.QuickTip,
                Headline = "Redis key counts after purging",
                ContentAsHtml = true,
                Content = HtmlParagraphs(
                    "Fusion Cache purging invalidates cached entries; it does not immediately delete their keys from Redis. " +
                        "Invalidated entries are no longer returned by Fusion Cache, but their keys may remain until expiry or subsequent access.")
            });
        }
        else
        {
            PageConfiguration.Callouts.Add(new CalloutConfiguration
            {
                Type = CalloutType.QuickTip,
                Headline = "Always excluded from direct Redis deletion",
                ContentAsHtml = true,
                Content = HtmlLines(
                    "Exact keys: " + Rules(cacheManagement.ExcludedKeys),
                    "Patterns: " + Rules(cacheManagement.ExcludedPatterns))
            });
        }

        PageConfiguration.SubmitConfiguration.ConfirmationConfiguration = new ConfirmationConfiguration
        {
            Title = PurgeAllCacheLayers ? "Purge All layers (L1 + L2)?" : "Purge the connected Redis database?",
            Detail = PurgeAllCacheLayers
                ? "All Fusion Cache entries will be invalidated in memory and Redis across the shared backplane, including stale fallback entries. This cannot be undone."
                : "All Redis keys other than the configured and entered exclusions will be deleted. This cannot be undone.",
            Button = actionLabel
        };

        if (!cacheManagement.IsEnabled)
        {
            PageConfiguration.ErrorMessage = "Redis management is unavailable because DevMode uses memory caching only.";
        }
    }

    [PageCommand(Permission = RedisCacheApplication.PurgePermission)]
    public override Task<ICommandResponse> Submit(FormSubmissionCommandArguments args) => base.Submit(args);

    [PageCommand(Permission = RedisCacheApplication.PurgePermission)]
    public override Task<ICommandResponse<FormChangeResult>> Change(FormChangeCommandArguments args) => base.Change(args);

    protected override async Task<ICommandResponse> ProcessFormData(RedisCachePurgeModel model, ICollection<IFormItem> formItems)
    {
        if (!string.Equals(model.Confirmation, "PURGE", StringComparison.Ordinal))
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage(PurgeAllCacheLayers
                    ? "Type PURGE to confirm invalidation of all Fusion Cache entries in L1 + L2."
                    : "Type PURGE to confirm deletion of the connected Redis database's keys.");
        }

        // Resolve the actor before issuing any destructive commands.
        var user = await userAccessor.Get();
        RedisCachePurgeResult result;
        try
        {
            result = PurgeAllCacheLayers
                ? await cacheManagement.PurgeAllLayersAsync()
                : await cacheManagement.PurgeAsync(ParseLines(model.ExcludedKeys), ParseLines(model.ExcludedPatterns));
        }
        catch (ArgumentException exception)
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure)).AddErrorMessage(exception.Message);
        }

        string eventCode = PurgeAllCacheLayers ? "PURGEALL" : "PURGE";
        string scope = PurgeAllCacheLayers ? "Fusion Cache L1 + L2 across applications" : "Redis";
        string message = PurgeAllCacheLayers ? result.Message
            : $"{result.Message} Deleted: {result.DeletedKeys:N0}. Excluded key observations: {result.ExcludedKeyObservations:N0}.";
        eventLog.LogInformation("FusionCache.Redis", result.Completed ? eventCode : eventCode + "INCOMPLETE",
            $"User ID: {user.UserID}. Scope: {scope}. {message}");

        var response = ResponseFrom(new FormSubmissionResult(result.Completed
            ? FormSubmissionStatus.ValidationSuccess : FormSubmissionStatus.ValidationFailure)
        {
            Items = await formItems.OnlyVisible().GetClientProperties()
        });
        return result.Completed ? response.AddSuccessMessage(message) : response.AddErrorMessage(message);
    }

    internal static string[] ParseLines(string? value) => (value ?? "").Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries);
    private static string Rules(IReadOnlyList<string> rules) => rules.Count == 0 ? "None" : string.Join(", ", rules);
    private static string HtmlLines(params string[] lines) =>
        string.Join("<br />", lines.Select(line => HtmlEncoder.Default.Encode(line)));
    private static string HtmlParagraphs(params string[] paragraphs) =>
        string.Join("<br /><br />", paragraphs.Select(paragraph => HtmlEncoder.Default.Encode(paragraph)));
}

public sealed class RedisCachePurgeModel
{
    [TextAreaComponent(Label = "Additional Redis keys to exclude from deletion", Order = 1,
        ExplanationText = "One key per line. Case-sensitive; spaces and wildcard characters are treated literally. Applies to direct Redis deletion for this purge only.")]
    public string ExcludedKeys { get; set; } = "";

    [TextAreaComponent(Label = "Additional Redis key patterns to exclude from deletion", Order = 2,
        ExplanationText = "One pattern per line, for example session:* or *:settings. * matches any characters; ? matches one character. All other characters are literal. Matching is case-sensitive.")]
    public string ExcludedPatterns { get; set; } = "";

    [TextInputComponent(Label = "Type PURGE to confirm", Order = 3)]
    [RequiredValidationRule]
    public string Confirmation { get; set; } = "";
}
