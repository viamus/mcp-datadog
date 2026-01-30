using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using Viamus.DataDog.Mcp.Server.Models;
using Viamus.DataDog.Mcp.Server.Services;
using Viamus.DataDog.Mcp.Server.Tools;

namespace Viamus.DataDog.Mcp.Server.Tests.Tools;

public class MonitorsToolTests
{
    private readonly IDatadogClient _mockClient;
    private readonly ILogger<MonitorsTool> _mockLogger;
    private readonly MonitorsTool _tool;

    public MonitorsToolTests()
    {
        _mockClient = Substitute.For<IDatadogClient>();
        _mockLogger = Substitute.For<ILogger<MonitorsTool>>();
        _tool = new MonitorsTool(_mockClient, _mockLogger);
    }

    [Fact]
    public async Task ListMonitorsAsync_ReturnsMonitors_WhenMonitorsExist()
    {
        // Arrange
        var monitors = new List<DatadogMonitor>
        {
            new()
            {
                Id = 123,
                Name = "CPU Monitor",
                Type = "metric alert",
                OverallState = "OK",
                Priority = 3,
                Tags = new List<string> { "env:prod" },
                Created = DateTime.UtcNow.AddDays(-30),
                Creator = new MonitorCreator { Name = "John Doe", Email = "john@example.com" }
            },
            new()
            {
                Id = 456,
                Name = "Memory Monitor",
                Type = "metric alert",
                OverallState = "Alert",
                Priority = 1,
                Tags = new List<string> { "env:prod", "team:backend" }
            }
        };

        _mockClient.GetMonitorsAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>()
        ).Returns(monitors);

        // Act
        var result = await _tool.ListMonitorsAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("monitors").EnumerateArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task ListMonitorsAsync_PassesFilters_Correctly()
    {
        // Arrange
        string? capturedName = null;
        string? capturedTags = null;

        _mockClient.GetMonitorsAsync(
            Arg.Any<string?>(),
            Arg.Do<string?>(n => capturedName = n),
            Arg.Do<string?>(t => capturedTags = t),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<DatadogMonitor>());

        // Act
        await _tool.ListMonitorsAsync(name: "CPU*", tags: "env:prod");

        // Assert
        capturedName.Should().Be("CPU*");
        capturedTags.Should().Be("env:prod");
    }

    [Fact]
    public async Task ListMonitorsAsync_ReturnsError_WhenHttpRequestFails()
    {
        // Arrange
        _mockClient.GetMonitorsAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>()
        ).Throws(new HttpRequestException("API error"));

        // Act
        var result = await _tool.ListMonitorsAsync();

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetString().Should().Contain("Failed to list monitors");
    }

    [Fact]
    public async Task GetMonitorAsync_ReturnsMonitorDetails_WhenMonitorExists()
    {
        // Arrange
        var monitor = new DatadogMonitor
        {
            Id = 123,
            Name = "CPU Monitor",
            Type = "metric alert",
            Query = "avg(last_5m):avg:system.cpu.user{*} > 90",
            Message = "CPU usage is high",
            OverallState = "OK",
            Priority = 2,
            Tags = new List<string> { "env:prod" },
            Options = new MonitorOptions
            {
                NotifyNoData = true,
                NoDataTimeframe = 10,
                Thresholds = new MonitorThresholds
                {
                    Critical = 90,
                    Warning = 80
                }
            }
        };

        _mockClient.GetMonitorAsync(123, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(monitor);

        // Act
        var result = await _tool.GetMonitorAsync(123);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("id").GetInt64().Should().Be(123);
        json.RootElement.GetProperty("name").GetString().Should().Be("CPU Monitor");
        json.RootElement.GetProperty("query").GetString().Should().Contain("system.cpu.user");
    }

    [Fact]
    public async Task GetMonitorAsync_ReturnsError_WhenMonitorNotFound()
    {
        // Arrange
        _mockClient.GetMonitorAsync(999, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Not found"));

        // Act
        var result = await _tool.GetMonitorAsync(999);

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ListMonitorsAsync_ClampsPageSize_ToValidRange()
    {
        // Arrange
        int? capturedPageSize = null;

        _mockClient.GetMonitorsAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Do<int?>(p => capturedPageSize = p),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<DatadogMonitor>());

        // Act
        await _tool.ListMonitorsAsync(pageSize: 5000);

        // Assert
        capturedPageSize.Should().Be(1000); // Clamped to max
    }
}
