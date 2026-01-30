using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.DataDog.Mcp.Server.Services;

namespace Viamus.DataDog.Mcp.Server.Tools;

[McpServerToolType]
public sealed class MetricsTool(IDatadogClient datadogClient, ILogger<MetricsTool> logger)
{
    [McpServerTool(Name = "query_metrics")]
    [Description("Query metrics from Datadog. Use this to retrieve time-series data for specific metrics.")]
    public async Task<string> QueryMetricsAsync(
        [Description("The metrics query (e.g., 'avg:system.cpu.user{*}', 'sum:my.metric{env:prod} by {host}')")]
        string query,

        [Description("Start time (ISO 8601 format or relative like '1h', '1d', '7d'). Default: 1 hour ago")]
        string? from = null,

        [Description("End time (ISO 8601 format or 'now'). Default: now")]
        string? to = null,

        CancellationToken cancellationToken = default)
    {
        try
        {
            var fromTime = ParseTime(from, TimeSpan.FromHours(-1));
            var toTime = ParseTime(to, TimeSpan.Zero);

            logger.LogInformation("Querying metrics: query={Query}, from={From}, to={To}", query, fromTime, toTime);

            var result = await datadogClient.QueryMetricsAsync(query, fromTime, toTime, cancellationToken);

            var response = new
            {
                status = result.Status,
                query = result.Query,
                fromDate = DateTimeOffset.FromUnixTimeMilliseconds(result.FromDate).DateTime,
                toDate = DateTimeOffset.FromUnixTimeMilliseconds(result.ToDate).DateTime,
                series = result.Series?.Select(s => new
                {
                    metric = s.Metric,
                    displayName = s.DisplayName,
                    scope = s.Scope,
                    tags = s.TagSet,
                    aggregation = s.Aggregation,
                    unit = s.Unit?.FirstOrDefault()?.ShortName,
                    pointCount = s.PointList?.Count ?? 0,
                    points = s.PointList?.Take(100).Select(p => new
                    {
                        timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)p[0]).DateTime,
                        value = p.Count > 1 ? p[1] : 0
                    })
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to query metrics");
            return JsonSerializer.Serialize(new { error = $"Failed to query metrics: {ex.Message}" });
        }
    }

    [McpServerTool(Name = "list_metrics")]
    [Description("List available metric names in Datadog. Use this to discover what metrics are available.")]
    public async Task<string> ListMetricsAsync(
        [Description("Filter metrics by host name")]
        string? host = null,

        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Listing metrics for host: {Host}", host ?? "all");

            var metrics = await datadogClient.GetMetricNamesAsync(host, cancellationToken);

            var response = new
            {
                total = metrics.Count,
                metrics = metrics.Take(500) // Limit to first 500 metrics
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to list metrics");
            return JsonSerializer.Serialize(new { error = $"Failed to list metrics: {ex.Message}" });
        }
    }

    [McpServerTool(Name = "get_metric_metadata")]
    [Description("Get metadata for a specific metric including type, description, and unit.")]
    public async Task<string> GetMetricMetadataAsync(
        [Description("The metric name (e.g., 'system.cpu.user', 'aws.ec2.cpuutilization')")]
        string metricName,

        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Getting metadata for metric: {MetricName}", metricName);

            var metadata = await datadogClient.GetMetricMetadataAsync(metricName, cancellationToken);

            var response = new
            {
                name = metricName,
                type = metadata.Type,
                description = metadata.Description,
                shortName = metadata.ShortName,
                unit = metadata.Unit,
                perUnit = metadata.PerUnit,
                statsdInterval = metadata.StatsdInterval
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get metric metadata for {MetricName}", metricName);
            return JsonSerializer.Serialize(new { error = $"Failed to get metric metadata: {ex.Message}" });
        }
    }

    private static DateTime ParseTime(string? timeStr, TimeSpan defaultOffset)
    {
        if (string.IsNullOrEmpty(timeStr))
            return DateTime.UtcNow.Add(defaultOffset);

        if (timeStr.Equals("now", StringComparison.OrdinalIgnoreCase))
            return DateTime.UtcNow;

        if (DateTime.TryParse(timeStr, out var parsed))
            return parsed.ToUniversalTime();

        if (TryParseRelativeTime(timeStr, out var offset))
            return DateTime.UtcNow.Add(-offset);

        return DateTime.UtcNow.Add(defaultOffset);
    }

    private static bool TryParseRelativeTime(string input, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        if (string.IsNullOrEmpty(input) || input.Length < 2)
            return false;

        var unit = input[^1];
        if (!int.TryParse(input[..^1], out var value))
            return false;

        result = unit switch
        {
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            'w' => TimeSpan.FromDays(value * 7),
            _ => TimeSpan.Zero
        };

        return result != TimeSpan.Zero;
    }
}
