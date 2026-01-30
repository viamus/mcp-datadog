using System.Text.Json.Serialization;

namespace Viamus.DataDog.Mcp.Server.Models;

public class IncidentListResponse
{
    [JsonPropertyName("data")]
    public List<Incident>? Data { get; set; }

    [JsonPropertyName("meta")]
    public IncidentMeta? Meta { get; set; }
}

public class Incident
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("attributes")]
    public IncidentAttributes? Attributes { get; set; }

    [JsonPropertyName("relationships")]
    public IncidentRelationships? Relationships { get; set; }
}

public class IncidentAttributes
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("customer_impact_scope")]
    public string? CustomerImpactScope { get; set; }

    [JsonPropertyName("customer_impact_start")]
    public DateTime? CustomerImpactStart { get; set; }

    [JsonPropertyName("customer_impact_end")]
    public DateTime? CustomerImpactEnd { get; set; }

    [JsonPropertyName("customer_impacted")]
    public bool CustomerImpacted { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; }

    [JsonPropertyName("detected")]
    public DateTime? Detected { get; set; }

    [JsonPropertyName("resolved")]
    public DateTime? Resolved { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("time_to_detect")]
    public long? TimeToDetect { get; set; }

    [JsonPropertyName("time_to_internal_response")]
    public long? TimeToInternalResponse { get; set; }

    [JsonPropertyName("time_to_repair")]
    public long? TimeToRepair { get; set; }

    [JsonPropertyName("time_to_resolve")]
    public long? TimeToResolve { get; set; }

    [JsonPropertyName("public_id")]
    public long PublicId { get; set; }

    [JsonPropertyName("notification_handles")]
    public List<string>? NotificationHandles { get; set; }

    [JsonPropertyName("fields")]
    public Dictionary<string, IncidentFieldValue>? Fields { get; set; }
}

public class IncidentFieldValue
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public class IncidentRelationships
{
    [JsonPropertyName("commander_user")]
    public IncidentRelationshipData? CommanderUser { get; set; }

    [JsonPropertyName("created_by_user")]
    public IncidentRelationshipData? CreatedByUser { get; set; }

    [JsonPropertyName("last_modified_by_user")]
    public IncidentRelationshipData? LastModifiedByUser { get; set; }
}

public class IncidentRelationshipData
{
    [JsonPropertyName("data")]
    public IncidentRelationshipItem? Data { get; set; }
}

public class IncidentRelationshipItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class IncidentMeta
{
    [JsonPropertyName("pagination")]
    public IncidentPagination? Pagination { get; set; }
}

public class IncidentPagination
{
    [JsonPropertyName("offset")]
    public long Offset { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("next_offset")]
    public long? NextOffset { get; set; }
}
