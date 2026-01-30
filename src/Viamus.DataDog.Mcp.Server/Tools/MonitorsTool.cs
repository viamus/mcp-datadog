using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.DataDog.Mcp.Server.Services;

namespace Viamus.DataDog.Mcp.Server.Tools;

[McpServerToolType]
public sealed class MonitorsTool(IDatadogClient datadogClient, ILogger<MonitorsTool> logger)
{
    [McpServerTool(Name = "list_monitors")]
    [Description("List all monitors in Datadog. Can filter by name, tags, or state. Use this to get an overview of monitoring alerts.")]
    public async Task<string> ListMonitorsAsync(
        [Description("Filter monitors by name (supports wildcards)")]
        string? name = null,

        [Description("Filter monitors by tags (comma-separated). Example: 'env:prod,team:backend'")]
        string? tags = null,

        [Description("Filter monitors by monitor tags (comma-separated)")]
        string? monitorTags = null,

        [Description("Include group states in response. Comma-separated: 'all', 'alert', 'warn', 'no data', 'ok'")]
        string? groupStates = null,

        [Description("Include downtime information")]
        bool withDowntimes = false,

        [Description("Number of monitors per page (1-1000). Default: 100")]
        int pageSize = 100,

        [Description("Page number (0-indexed). Default: 0")]
        int page = 0,

        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 1000);

            logger.LogInformation("Listing monitors: name={Name}, tags={Tags}, page={Page}, pageSize={PageSize}",
                name, tags, page, pageSize);

            var monitors = await datadogClient.GetMonitorsAsync(
                groupStates,
                name,
                tags,
                monitorTags,
                withDowntimes,
                pageSize,
                page,
                cancellationToken);

            var response = new
            {
                total = monitors.Count,
                page,
                pageSize,
                monitors = monitors.Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    type = m.Type,
                    overallState = m.OverallState,
                    priority = m.Priority,
                    tags = m.Tags,
                    created = m.Created,
                    modified = m.Modified,
                    creator = m.Creator?.Name ?? m.Creator?.Email
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to list monitors");
            return JsonSerializer.Serialize(new { error = $"Failed to list monitors: {ex.Message}" });
        }
    }

    [McpServerTool(Name = "get_monitor")]
    [Description("Get detailed information about a specific monitor by ID. Use this to see full monitor configuration, thresholds, and current state.")]
    public async Task<string> GetMonitorAsync(
        [Description("The monitor ID")]
        long monitorId,

        [Description("Include group states. Comma-separated: 'all', 'alert', 'warn', 'no data', 'ok'")]
        string? groupStates = null,

        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Getting monitor: id={MonitorId}", monitorId);

            var monitor = await datadogClient.GetMonitorAsync(monitorId, groupStates, cancellationToken);

            var response = new
            {
                id = monitor.Id,
                name = monitor.Name,
                type = monitor.Type,
                query = monitor.Query,
                message = monitor.Message,
                overallState = monitor.OverallState,
                priority = monitor.Priority,
                tags = monitor.Tags,
                created = monitor.Created,
                modified = monitor.Modified,
                creator = new
                {
                    name = monitor.Creator?.Name,
                    email = monitor.Creator?.Email
                },
                options = new
                {
                    notifyNoData = monitor.Options?.NotifyNoData,
                    noDataTimeframe = monitor.Options?.NoDataTimeframe,
                    renotifyInterval = monitor.Options?.RenotifyInterval,
                    timeoutHours = monitor.Options?.TimeoutHours,
                    thresholds = monitor.Options?.Thresholds != null ? new
                    {
                        critical = monitor.Options.Thresholds.Critical,
                        warning = monitor.Options.Thresholds.Warning,
                        ok = monitor.Options.Thresholds.Ok
                    } : null
                },
                state = monitor.State?.Groups?.Select(g => new
                {
                    group = g.Key,
                    status = g.Value.Status,
                    lastTriggered = g.Value.LastTriggeredTimestamp.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(g.Value.LastTriggeredTimestamp.Value).DateTime
                        : (DateTime?)null
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get monitor {MonitorId}", monitorId);
            return JsonSerializer.Serialize(new { error = $"Failed to get monitor: {ex.Message}" });
        }
    }
}
