using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.DataDog.Mcp.Server.Services;

namespace Viamus.DataDog.Mcp.Server.Tools;

[McpServerToolType]
public sealed class SearchLogsTool(IDatadogClient datadogClient, ILogger<SearchLogsTool> logger)
{
    [McpServerTool(Name = "search_logs")]
    [Description("Search logs in Datadog. Use this to find log entries matching specific queries, services, hosts, or time ranges.")]
    public async Task<string> SearchLogsAsync(
        [Description("The search query (Datadog log query syntax). Example: 'service:my-app status:error'")]
        string query,

        [Description("Start time for the search (ISO 8601 format or relative like '15m', '1h', '1d'). Default: 15 minutes ago")]
        string? from = null,

        [Description("End time for the search (ISO 8601 format or 'now'). Default: now")]
        string? to = null,

        [Description("Maximum number of logs to return (1-1000). Default: 100")]
        int limit = 100,

        [Description("Sort order: 'timestamp' (ascending) or '-timestamp' (descending). Default: -timestamp")]
        string? sort = null,

        CancellationToken cancellationToken = default)
    {
        try
        {
            var fromTime = ParseTime(from, TimeSpan.FromMinutes(-15));
            var toTime = ParseTime(to, TimeSpan.Zero);

            limit = Math.Clamp(limit, 1, 1000);
            sort ??= "-timestamp";

            logger.LogInformation("Searching logs: query={Query}, from={From}, to={To}, limit={Limit}",
                query, fromTime, toTime, limit);

            var result = await datadogClient.SearchLogsAsync(
                query,
                fromTime,
                toTime,
                limit,
                sort,
                cancellationToken: cancellationToken);

            var response = new
            {
                total = result.Data?.Count ?? 0,
                logs = result.Data?.Select(log => new
                {
                    id = log.Id,
                    timestamp = log.Attributes?.Timestamp,
                    status = log.Attributes?.Status,
                    service = log.Attributes?.Service,
                    host = log.Attributes?.Host,
                    message = log.Attributes?.Message,
                    tags = log.Attributes?.Tags
                }),
                hasMore = !string.IsNullOrEmpty(result.Meta?.Page?.After),
                cursor = result.Meta?.Page?.After
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to search logs");
            return JsonSerializer.Serialize(new { error = $"Failed to search logs: {ex.Message}" });
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

        // Parse relative time (e.g., "15m", "1h", "1d")
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
