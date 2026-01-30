using System.Text.Json.Serialization;

namespace Viamus.DataDog.Mcp.Server.Models;

public class DatadogMonitor
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("overall_state")]
    public string? OverallState { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTime? Modified { get; set; }

    [JsonPropertyName("creator")]
    public MonitorCreator? Creator { get; set; }

    [JsonPropertyName("options")]
    public MonitorOptions? Options { get; set; }

    [JsonPropertyName("state")]
    public MonitorState? State { get; set; }
}

public class MonitorCreator
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class MonitorOptions
{
    [JsonPropertyName("notify_no_data")]
    public bool NotifyNoData { get; set; }

    [JsonPropertyName("no_data_timeframe")]
    public int? NoDataTimeframe { get; set; }

    [JsonPropertyName("notify_audit")]
    public bool NotifyAudit { get; set; }

    [JsonPropertyName("timeout_h")]
    public int? TimeoutHours { get; set; }

    [JsonPropertyName("renotify_interval")]
    public int? RenotifyInterval { get; set; }

    [JsonPropertyName("escalation_message")]
    public string? EscalationMessage { get; set; }

    [JsonPropertyName("include_tags")]
    public bool IncludeTags { get; set; }

    [JsonPropertyName("thresholds")]
    public MonitorThresholds? Thresholds { get; set; }
}

public class MonitorThresholds
{
    [JsonPropertyName("critical")]
    public double? Critical { get; set; }

    [JsonPropertyName("warning")]
    public double? Warning { get; set; }

    [JsonPropertyName("ok")]
    public double? Ok { get; set; }

    [JsonPropertyName("critical_recovery")]
    public double? CriticalRecovery { get; set; }

    [JsonPropertyName("warning_recovery")]
    public double? WarningRecovery { get; set; }
}

public class MonitorState
{
    [JsonPropertyName("groups")]
    public Dictionary<string, MonitorGroupState>? Groups { get; set; }
}

public class MonitorGroupState
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("last_triggered_ts")]
    public long? LastTriggeredTimestamp { get; set; }

    [JsonPropertyName("last_nodata_ts")]
    public long? LastNoDataTimestamp { get; set; }

    [JsonPropertyName("last_notified_ts")]
    public long? LastNotifiedTimestamp { get; set; }

    [JsonPropertyName("last_resolved_ts")]
    public long? LastResolvedTimestamp { get; set; }
}
