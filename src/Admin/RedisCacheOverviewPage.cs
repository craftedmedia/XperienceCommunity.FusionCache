using System.Globalization;

using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;

using XperienceCommunity.FusionCache.Admin;
using XperienceCommunity.FusionCache.Management;

[assembly: UIPage(typeof(RedisCacheApplication), "overview", typeof(RedisCacheOverviewPage),
    "Overview", TemplateNames.EDIT, 0)]

namespace XperienceCommunity.FusionCache.Admin;

[UIEvaluatePermission(SystemPermissions.VIEW)]
public sealed class RedisCacheOverviewPage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IRedisCacheManagementService cacheManagement)
    : ModelEditPage<RedisCacheOverviewModel>(formItemCollectionProvider, formDataBinder)
{
    protected override RedisCacheOverviewModel Model { get; } = new();

    public override async Task ConfigurePage()
    {
        var overview = await cacheManagement.GetOverviewAsync();

        Model.Name = overview.Name;
        Model.InstanceName = overview.InstanceName ?? "";
        Model.Status = overview.Status;
        Model.Database = overview.Database?.ToString(CultureInfo.InvariantCulture) ?? "";
        Model.Keys = Number(overview.TotalKeys);
        Model.Memory = overview.Servers.Count > 0 && overview.Servers.All(server => server.UsedMemoryBytes.HasValue)
            ? Megabytes(overview.Servers.Sum(server => server.UsedMemoryBytes!.Value)) : "";
        Model.Servers = string.Join(Environment.NewLine + Environment.NewLine, overview.Servers.Select(ServerDetails));
        Model.Updated = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

        // Kentico captures the model's client form values in this call.
        await base.ConfigurePage();

        PageConfiguration.Headline = "Redis overview";
        PageConfiguration.EditMode = FormEditMode.ReadOnly;
        PageConfiguration.SubmitConfiguration.Visible = false;
        PageConfiguration.Callouts.Add(new CalloutConfiguration
        {
            Type = CalloutType.QuickTip,
            Content = "Key counts cover the database used by FusionCache. Memory, uptime and client counts cover each entire Redis server. Refresh this page to update the snapshot."
        });
    }

    protected override async Task<IEnumerable<IFormItemClientProperties>> GetFormItemsClientProperties() =>
        (await base.GetFormItemsClientProperties()).Where(item =>
            item is not IFormComponentClientProperties component ||
            component.Value is string value && !string.IsNullOrWhiteSpace(value));

    // A forged form submission must not change anything on this read-only page.
    [PageCommand(Permission = SystemPermissions.VIEW)]
    public override Task<ICommandResponse> Submit(FormSubmissionCommandArguments args) =>
        Task.FromResult<ICommandResponse>(ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
            .AddErrorMessage("The overview is read-only."));

    private static string ServerDetails(RedisServerOverview server)
    {
        var lines = new List<string> { server.Endpoint };

        if (!string.IsNullOrWhiteSpace(server.Version))
        {
            lines.Add($"Redis version: {server.Version}");
        }

        if (server.UsedMemoryBytes.HasValue)
        {
            lines.Add($"Memory: {Megabytes(server.UsedMemoryBytes)}");
        }

        if (server.PeakMemoryBytes.HasValue)
        {
            lines.Add($"Peak memory: {Megabytes(server.PeakMemoryBytes)}");
        }

        if (server.ConnectedClients.HasValue)
        {
            lines.Add($"Connected clients: {Number(server.ConnectedClients)}");
        }

        if (server.UptimeSeconds.HasValue)
        {
            lines.Add($"Uptime: {Uptime(server.UptimeSeconds.Value)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Number(long? value) => value?.ToString("N0", CultureInfo.CurrentCulture) ?? "";
    private static string Megabytes(long? bytes) => bytes.HasValue
        ? $"{bytes.Value / 1024d / 1024d:N2} MiB" : "";
    private static string Uptime(long seconds) =>
        $"{seconds / 86400}d {seconds % 86400 / 3600}h {seconds % 3600 / 60}m";
}

public sealed class RedisCacheOverviewModel
{
    [TextInputComponent(Label = "Redis name", Order = 1)]
    public string Name { get; set; } = "";

    [TextInputComponent(Label = "Redis instance name", Order = 2)]
    public string InstanceName { get; set; } = "";

    [TextInputComponent(Label = "Connection status", Order = 3)]
    public string Status { get; set; } = "";

    [TextInputComponent(Label = "Database", Order = 4)]
    public string Database { get; set; } = "";

    [TextInputComponent(Label = "Total keys in database", Order = 5)]
    public string Keys { get; set; } = "";

    [TextInputComponent(Label = "Memory used by primary servers", Order = 6)]
    public string Memory { get; set; } = "";

    [TextAreaComponent(Label = "Primary servers", Order = 7, MinRowsNumber = 8, MaxRowsNumber = 30)]
    public string Servers { get; set; } = "";

    [TextInputComponent(Label = "Last updated", Order = 8)]
    public string Updated { get; set; } = "";
}
