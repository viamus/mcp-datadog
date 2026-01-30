using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.DataDog.Mcp.Server.Services;

namespace Viamus.DataDog.Mcp.Server.Tools;

[McpServerToolType]
public sealed class DashboardsTool(IDatadogClient datadogClient, ILogger<DashboardsTool> logger)
{
    [McpServerTool(Name = "list_dashboards")]
    [Description("List all dashboards in Datadog. Use this to discover available dashboards and their IDs.")]
    public async Task<string> ListDashboardsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Listing dashboards");

            var result = await datadogClient.GetDashboardsAsync(cancellationToken);

            var response = new
            {
                total = result.Dashboards?.Count ?? 0,
                dashboards = result.Dashboards?.Select(d => new
                {
                    id = d.Id,
                    title = d.Title,
                    description = d.Description,
                    layoutType = d.LayoutType,
                    url = d.Url,
                    isReadOnly = d.IsReadOnly,
                    authorHandle = d.AuthorHandle,
                    createdAt = d.CreatedAt,
                    modifiedAt = d.ModifiedAt
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to list dashboards");
            return JsonSerializer.Serialize(new { error = $"Failed to list dashboards: {ex.Message}" });
        }
    }

    [McpServerTool(Name = "get_dashboard")]
    [Description("Get detailed information about a specific dashboard including widgets and configuration.")]
    public async Task<string> GetDashboardAsync(
        [Description("The dashboard ID")]
        string dashboardId,

        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Getting dashboard: {DashboardId}", dashboardId);

            var dashboard = await datadogClient.GetDashboardAsync(dashboardId, cancellationToken);

            var response = new
            {
                id = dashboard.Id,
                title = dashboard.Title,
                description = dashboard.Description,
                layoutType = dashboard.LayoutType,
                url = dashboard.Url,
                isReadOnly = dashboard.IsReadOnly,
                authorHandle = dashboard.AuthorHandle,
                createdAt = dashboard.CreatedAt,
                modifiedAt = dashboard.ModifiedAt,
                reflowType = dashboard.ReflowType,
                notifyList = dashboard.NotifyList,
                templateVariables = dashboard.TemplateVariables?.Select(tv => new
                {
                    name = tv.Name,
                    prefix = tv.Prefix,
                    defaultValue = tv.Default,
                    availableValues = tv.AvailableValues
                }),
                widgetCount = dashboard.Widgets?.Count ?? 0,
                widgets = dashboard.Widgets?.Select(w => new
                {
                    id = w.Id,
                    type = w.Definition?.Type,
                    title = w.Definition?.Title,
                    layout = w.Layout != null ? new
                    {
                        x = w.Layout.X,
                        y = w.Layout.Y,
                        width = w.Layout.Width,
                        height = w.Layout.Height
                    } : null,
                    queries = w.Definition?.Requests?.Select(r => r.Query)
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get dashboard {DashboardId}", dashboardId);
            return JsonSerializer.Serialize(new { error = $"Failed to get dashboard: {ex.Message}" });
        }
    }
}
