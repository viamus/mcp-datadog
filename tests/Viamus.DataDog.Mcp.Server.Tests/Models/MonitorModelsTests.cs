using System.Text.Json;
using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Tests.Models;

public class MonitorModelsTests
{
    [Fact]
    public void DatadogMonitor_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "id": 12345,
            "name": "CPU Monitor",
            "type": "metric alert",
            "query": "avg(last_5m):avg:system.cpu.user{*} > 90",
            "message": "CPU is high @oncall",
            "overall_state": "OK",
            "tags": ["env:prod", "team:infra"],
            "priority": 2,
            "created": "2024-01-01T00:00:00Z",
            "modified": "2024-01-15T10:00:00Z",
            "creator": {
                "email": "john@example.com",
                "handle": "john@example.com",
                "name": "John Doe"
            },
            "options": {
                "notify_no_data": true,
                "no_data_timeframe": 10,
                "notify_audit": false,
                "timeout_h": 24,
                "renotify_interval": 60,
                "include_tags": true,
                "thresholds": {
                    "critical": 90.0,
                    "warning": 80.0,
                    "ok": 70.0
                }
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<DatadogMonitor>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(12345);
        result.Name.Should().Be("CPU Monitor");
        result.Type.Should().Be("metric alert");
        result.OverallState.Should().Be("OK");
        result.Tags.Should().Contain("env:prod");
        result.Priority.Should().Be(2);
        result.Creator!.Name.Should().Be("John Doe");
        result.Options!.NotifyNoData.Should().BeTrue();
        result.Options.Thresholds!.Critical.Should().Be(90.0);
        result.Options.Thresholds.Warning.Should().Be(80.0);
    }

    [Fact]
    public void MonitorState_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "groups": {
                "host:web-01": {
                    "status": "Alert",
                    "last_triggered_ts": 1705312000,
                    "last_resolved_ts": null
                },
                "host:web-02": {
                    "status": "OK",
                    "last_triggered_ts": null,
                    "last_resolved_ts": 1705311000
                }
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<MonitorState>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Groups.Should().HaveCount(2);
        result.Groups!["host:web-01"].Status.Should().Be("Alert");
        result.Groups["host:web-01"].LastTriggeredTimestamp.Should().Be(1705312000);
        result.Groups["host:web-02"].Status.Should().Be("OK");
    }

    [Fact]
    public void MonitorThresholds_HandlesPartialData()
    {
        // Arrange
        var json = """
        {
            "critical": 95.0
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<MonitorThresholds>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Critical.Should().Be(95.0);
        result.Warning.Should().BeNull();
        result.Ok.Should().BeNull();
    }
}
