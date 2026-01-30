using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Viamus.DataDog.Mcp.Server.Configuration;
using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Services;

public class DatadogClient : IDatadogClient
{
    private readonly HttpClient _httpClient;
    private readonly DatadogSettings _settings;
    private readonly ILogger<DatadogClient> _logger;

    public DatadogClient(
        HttpClient httpClient,
        IOptions<DatadogSettings> settings,
        ILogger<DatadogClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("DD-API-KEY", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("DD-APPLICATION-KEY", _settings.ApplicationKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    #region Logs

    public async Task<LogSearchResponse> SearchLogsAsync(
        string query,
        DateTime from,
        DateTime to,
        int limit = 100,
        string? sort = null,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            filter = new
            {
                query,
                from = from.ToUniversalTime().ToString("o"),
                to = to.ToUniversalTime().ToString("o")
            },
            sort = sort ?? "timestamp",
            page = new
            {
                limit,
                cursor
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _logger.LogDebug("Searching logs with query: {Query}, from: {From}, to: {To}", query, from, to);

        var response = await _httpClient.PostAsync("/api/v2/logs/events/search", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LogSearchResponse>(cancellationToken: cancellationToken);
        return result ?? new LogSearchResponse();
    }

    #endregion

    #region Monitors

    public async Task<List<DatadogMonitor>> GetMonitorsAsync(
        string? groupStates = null,
        string? name = null,
        string? tags = null,
        string? monitorTags = null,
        bool withDowntimes = false,
        int? pageSize = null,
        int? page = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(groupStates))
            queryParams.Add($"group_states={Uri.EscapeDataString(groupStates)}");

        if (!string.IsNullOrEmpty(name))
            queryParams.Add($"name={Uri.EscapeDataString(name)}");

        if (!string.IsNullOrEmpty(tags))
            queryParams.Add($"tags={Uri.EscapeDataString(tags)}");

        if (!string.IsNullOrEmpty(monitorTags))
            queryParams.Add($"monitor_tags={Uri.EscapeDataString(monitorTags)}");

        if (withDowntimes)
            queryParams.Add("with_downtimes=true");

        if (pageSize.HasValue)
            queryParams.Add($"page_size={pageSize.Value}");

        if (page.HasValue)
            queryParams.Add($"page={page.Value}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var url = $"/api/v1/monitor{queryString}";

        _logger.LogDebug("Getting monitors from: {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<DatadogMonitor>>(cancellationToken: cancellationToken);
        return result ?? [];
    }

    public async Task<DatadogMonitor> GetMonitorAsync(
        long monitorId,
        string? groupStates = null,
        CancellationToken cancellationToken = default)
    {
        var queryString = !string.IsNullOrEmpty(groupStates)
            ? $"?group_states={Uri.EscapeDataString(groupStates)}"
            : "";

        var url = $"/api/v1/monitor/{monitorId}{queryString}";

        _logger.LogDebug("Getting monitor {MonitorId} from: {Url}", monitorId, url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DatadogMonitor>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException($"Monitor {monitorId} not found");
    }

    #endregion

    #region Metrics

    public async Task<MetricQueryResponse> QueryMetricsAsync(
        string query,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var fromUnix = new DateTimeOffset(from.ToUniversalTime()).ToUnixTimeSeconds();
        var toUnix = new DateTimeOffset(to.ToUniversalTime()).ToUnixTimeSeconds();

        var url = $"/api/v1/query?from={fromUnix}&to={toUnix}&query={Uri.EscapeDataString(query)}";

        _logger.LogDebug("Querying metrics: {Query}, from: {From}, to: {To}", query, from, to);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MetricQueryResponse>(cancellationToken: cancellationToken);
        return result ?? new MetricQueryResponse();
    }

    public async Task<List<string>> GetMetricNamesAsync(
        string? host = null,
        CancellationToken cancellationToken = default)
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
        var queryString = !string.IsNullOrEmpty(host)
            ? $"?from={from}&host={Uri.EscapeDataString(host)}"
            : $"?from={from}";

        var url = $"/api/v1/metrics{queryString}";

        _logger.LogDebug("Getting metric names from: {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MetricNamesResponse>(cancellationToken: cancellationToken);
        return result?.Metrics ?? [];
    }

    public async Task<MetricMetadata> GetMetricMetadataAsync(
        string metricName,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v1/metrics/{Uri.EscapeDataString(metricName)}";

        _logger.LogDebug("Getting metric metadata for: {MetricName}", metricName);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MetricMetadata>(cancellationToken: cancellationToken);
        return result ?? new MetricMetadata();
    }

    #endregion

    #region Incidents

    public async Task<IncidentListResponse> GetIncidentsAsync(
        int pageSize = 25,
        int pageOffset = 0,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v2/incidents?page[size]={pageSize}&page[offset]={pageOffset}";

        _logger.LogDebug("Getting incidents from: {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IncidentListResponse>(cancellationToken: cancellationToken);
        return result ?? new IncidentListResponse();
    }

    public async Task<Incident> GetIncidentAsync(
        string incidentId,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v2/incidents/{Uri.EscapeDataString(incidentId)}";

        _logger.LogDebug("Getting incident: {IncidentId}", incidentId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IncidentResponse>(cancellationToken: cancellationToken);
        return result?.Data ?? throw new InvalidOperationException($"Incident {incidentId} not found");
    }

    #endregion

    #region Dashboards

    public async Task<DashboardListResponse> GetDashboardsAsync(
        CancellationToken cancellationToken = default)
    {
        var url = "/api/v1/dashboard";

        _logger.LogDebug("Getting dashboards");

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DashboardListResponse>(cancellationToken: cancellationToken);
        return result ?? new DashboardListResponse();
    }

    public async Task<Dashboard> GetDashboardAsync(
        string dashboardId,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v1/dashboard/{Uri.EscapeDataString(dashboardId)}";

        _logger.LogDebug("Getting dashboard: {DashboardId}", dashboardId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dashboard>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException($"Dashboard {dashboardId} not found");
    }

    #endregion
}

// Helper classes for deserialization
internal class MetricNamesResponse
{
    public List<string>? Metrics { get; set; }
}

internal class IncidentResponse
{
    public Incident? Data { get; set; }
}
