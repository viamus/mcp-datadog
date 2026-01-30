using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.DataDog.Mcp.Server.Services;

namespace Viamus.DataDog.Mcp.Server.Tools;

[McpServerToolType]
public sealed class IncidentsTool(IDatadogClient datadogClient, ILogger<IncidentsTool> logger)
{
    [McpServerTool(Name = "list_incidents")]
    [Description("List incidents from Datadog. Use this to get an overview of ongoing and past incidents.")]
    public async Task<string> ListIncidentsAsync(
        [Description("Number of incidents per page (1-100). Default: 25")]
        int pageSize = 25,

        [Description("Page offset for pagination. Default: 0")]
        int pageOffset = 0,

        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            logger.LogInformation("Listing incidents: pageSize={PageSize}, pageOffset={PageOffset}", pageSize, pageOffset);

            var result = await datadogClient.GetIncidentsAsync(pageSize, pageOffset, cancellationToken);

            var response = new
            {
                total = result.Data?.Count ?? 0,
                pagination = new
                {
                    offset = result.Meta?.Pagination?.Offset,
                    size = result.Meta?.Pagination?.Size,
                    nextOffset = result.Meta?.Pagination?.NextOffset
                },
                incidents = result.Data?.Select(i => new
                {
                    id = i.Id,
                    publicId = i.Attributes?.PublicId,
                    title = i.Attributes?.Title,
                    severity = i.Attributes?.Severity,
                    state = i.Attributes?.State,
                    customerImpacted = i.Attributes?.CustomerImpacted,
                    customerImpactScope = i.Attributes?.CustomerImpactScope,
                    created = i.Attributes?.Created,
                    detected = i.Attributes?.Detected,
                    resolved = i.Attributes?.Resolved,
                    timeToDetect = i.Attributes?.TimeToDetect,
                    timeToResolve = i.Attributes?.TimeToResolve
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to list incidents");
            return JsonSerializer.Serialize(new { error = $"Failed to list incidents: {ex.Message}" });
        }
    }

    [McpServerTool(Name = "get_incident")]
    [Description("Get detailed information about a specific incident by ID.")]
    public async Task<string> GetIncidentAsync(
        [Description("The incident ID")]
        string incidentId,

        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Getting incident: {IncidentId}", incidentId);

            var incident = await datadogClient.GetIncidentAsync(incidentId, cancellationToken);

            var response = new
            {
                id = incident.Id,
                publicId = incident.Attributes?.PublicId,
                title = incident.Attributes?.Title,
                severity = incident.Attributes?.Severity,
                state = incident.Attributes?.State,
                customerImpacted = incident.Attributes?.CustomerImpacted,
                customerImpactScope = incident.Attributes?.CustomerImpactScope,
                customerImpactStart = incident.Attributes?.CustomerImpactStart,
                customerImpactEnd = incident.Attributes?.CustomerImpactEnd,
                created = incident.Attributes?.Created,
                modified = incident.Attributes?.Modified,
                detected = incident.Attributes?.Detected,
                resolved = incident.Attributes?.Resolved,
                timeToDetect = incident.Attributes?.TimeToDetect,
                timeToInternalResponse = incident.Attributes?.TimeToInternalResponse,
                timeToRepair = incident.Attributes?.TimeToRepair,
                timeToResolve = incident.Attributes?.TimeToResolve,
                notificationHandles = incident.Attributes?.NotificationHandles,
                fields = incident.Attributes?.Fields,
                relationships = new
                {
                    commander = incident.Relationships?.CommanderUser?.Data?.Id,
                    createdBy = incident.Relationships?.CreatedByUser?.Data?.Id,
                    lastModifiedBy = incident.Relationships?.LastModifiedByUser?.Data?.Id
                }
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get incident {IncidentId}", incidentId);
            return JsonSerializer.Serialize(new { error = $"Failed to get incident: {ex.Message}" });
        }
    }
}
