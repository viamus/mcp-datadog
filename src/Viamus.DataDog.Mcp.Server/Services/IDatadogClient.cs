using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Services;

public interface IDatadogClient
{
    // Logs
    Task<LogSearchResponse> SearchLogsAsync(
        string query,
        DateTime from,
        DateTime to,
        int limit = 100,
        string? sort = null,
        string? cursor = null,
        CancellationToken cancellationToken = default);

    // Monitors
    Task<List<DatadogMonitor>> GetMonitorsAsync(
        string? groupStates = null,
        string? name = null,
        string? tags = null,
        string? monitorTags = null,
        bool withDowntimes = false,
        int? pageSize = null,
        int? page = null,
        CancellationToken cancellationToken = default);

    Task<DatadogMonitor> GetMonitorAsync(
        long monitorId,
        string? groupStates = null,
        CancellationToken cancellationToken = default);

    // Metrics
    Task<MetricQueryResponse> QueryMetricsAsync(
        string query,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetMetricNamesAsync(
        string? host = null,
        CancellationToken cancellationToken = default);

    Task<MetricMetadata> GetMetricMetadataAsync(
        string metricName,
        CancellationToken cancellationToken = default);

    // Incidents
    Task<IncidentListResponse> GetIncidentsAsync(
        int pageSize = 25,
        int pageOffset = 0,
        CancellationToken cancellationToken = default);

    Task<Incident> GetIncidentAsync(
        string incidentId,
        CancellationToken cancellationToken = default);

    // Dashboards
    Task<DashboardListResponse> GetDashboardsAsync(
        CancellationToken cancellationToken = default);

    Task<Dashboard> GetDashboardAsync(
        string dashboardId,
        CancellationToken cancellationToken = default);
}
