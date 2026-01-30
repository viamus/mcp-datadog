using System.Text.Json.Serialization;

namespace Viamus.DataDog.Mcp.Server.Models;

public class LogSearchResponse
{
    [JsonPropertyName("data")]
    public List<LogEntry>? Data { get; set; }

    [JsonPropertyName("meta")]
    public LogMeta? Meta { get; set; }
}

public class LogEntry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("attributes")]
    public LogAttributes? Attributes { get; set; }
}

public class LogAttributes
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("service")]
    public string? Service { get; set; }

    [JsonPropertyName("host")]
    public string? Host { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, object>? CustomAttributes { get; set; }
}

public class LogMeta
{
    [JsonPropertyName("page")]
    public LogPage? Page { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("elapsed")]
    public long Elapsed { get; set; }
}

public class LogPage
{
    [JsonPropertyName("after")]
    public string? After { get; set; }
}
