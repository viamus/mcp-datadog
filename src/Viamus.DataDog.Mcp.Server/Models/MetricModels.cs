using System.Text.Json.Serialization;

namespace Viamus.DataDog.Mcp.Server.Models;

public class MetricQueryResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("res_type")]
    public string? ResType { get; set; }

    [JsonPropertyName("from_date")]
    public long FromDate { get; set; }

    [JsonPropertyName("to_date")]
    public long ToDate { get; set; }

    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("series")]
    public List<MetricSeries>? Series { get; set; }
}

public class MetricSeries
{
    [JsonPropertyName("metric")]
    public string? Metric { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("unit")]
    public List<MetricUnit>? Unit { get; set; }

    [JsonPropertyName("pointlist")]
    public List<List<double>>? PointList { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("tag_set")]
    public List<string>? TagSet { get; set; }

    [JsonPropertyName("aggr")]
    public string? Aggregation { get; set; }
}

public class MetricUnit
{
    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("scale_factor")]
    public double ScaleFactor { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("short_name")]
    public string? ShortName { get; set; }

    [JsonPropertyName("plural")]
    public string? Plural { get; set; }
}

public class MetricMetadata
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("short_name")]
    public string? ShortName { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("per_unit")]
    public string? PerUnit { get; set; }

    [JsonPropertyName("statsd_interval")]
    public int? StatsdInterval { get; set; }
}
