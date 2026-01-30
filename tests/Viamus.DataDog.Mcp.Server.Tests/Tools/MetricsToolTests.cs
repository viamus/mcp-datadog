using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using Viamus.DataDog.Mcp.Server.Models;
using Viamus.DataDog.Mcp.Server.Services;
using Viamus.DataDog.Mcp.Server.Tools;

namespace Viamus.DataDog.Mcp.Server.Tests.Tools;

public class MetricsToolTests
{
    private readonly IDatadogClient _mockClient;
    private readonly ILogger<MetricsTool> _mockLogger;
    private readonly MetricsTool _tool;

    public MetricsToolTests()
    {
        _mockClient = Substitute.For<IDatadogClient>();
        _mockLogger = Substitute.For<ILogger<MetricsTool>>();
        _tool = new MetricsTool(_mockClient, _mockLogger);
    }

    [Fact]
    public async Task QueryMetricsAsync_ReturnsMetricData_WhenQueryIsValid()
    {
        // Arrange
        var query = "avg:system.cpu.user{*}";
        var response = new MetricQueryResponse
        {
            Status = "ok",
            Query = query,
            FromDate = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(),
            ToDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Series = new List<MetricSeries>
            {
                new()
                {
                    Metric = "system.cpu.user",
                    DisplayName = "CPU User",
                    Scope = "host:web-01",
                    TagSet = new List<string> { "host:web-01" },
                    Aggregation = "avg",
                    PointList = new List<List<double>>
                    {
                        new() { DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 45.5 },
                        new() { DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds(), 42.3 }
                    },
                    Unit = new List<MetricUnit>
                    {
                        new() { ShortName = "%" }
                    }
                }
            }
        };

        _mockClient.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        // Act
        var result = await _tool.QueryMetricsAsync(query);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("status").GetString().Should().Be("ok");
        json.RootElement.GetProperty("series").EnumerateArray().Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryMetricsAsync_ReturnsError_WhenHttpRequestFails()
    {
        // Arrange
        _mockClient.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>()
        ).Throws(new HttpRequestException("API error"));

        // Act
        var result = await _tool.QueryMetricsAsync("avg:system.cpu.user{*}");

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetString().Should().Contain("Failed to query metrics");
    }

    [Fact]
    public async Task ListMetricsAsync_ReturnsMetricNames_WhenMetricsExist()
    {
        // Arrange
        var metrics = new List<string>
        {
            "system.cpu.user",
            "system.cpu.system",
            "system.mem.used",
            "system.disk.used"
        };

        _mockClient.GetMetricNamesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(metrics);

        // Act
        var result = await _tool.ListMetricsAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(4);
        json.RootElement.GetProperty("metrics").EnumerateArray().Should().HaveCount(4);
    }

    [Fact]
    public async Task ListMetricsAsync_PassesHostFilter_Correctly()
    {
        // Arrange
        string? capturedHost = null;

        _mockClient.GetMetricNamesAsync(
            Arg.Do<string?>(h => capturedHost = h),
            Arg.Any<CancellationToken>()
        ).Returns(new List<string>());

        // Act
        await _tool.ListMetricsAsync(host: "web-01");

        // Assert
        capturedHost.Should().Be("web-01");
    }

    [Fact]
    public async Task GetMetricMetadataAsync_ReturnsMetadata_WhenMetricExists()
    {
        // Arrange
        var metadata = new MetricMetadata
        {
            Type = "gauge",
            Description = "Percentage of CPU used by user processes",
            ShortName = "cpu user",
            Unit = "percent",
            PerUnit = null,
            StatsdInterval = 10
        };

        _mockClient.GetMetricMetadataAsync("system.cpu.user", Arg.Any<CancellationToken>())
            .Returns(metadata);

        // Act
        var result = await _tool.GetMetricMetadataAsync("system.cpu.user");

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("name").GetString().Should().Be("system.cpu.user");
        json.RootElement.GetProperty("type").GetString().Should().Be("gauge");
        json.RootElement.GetProperty("unit").GetString().Should().Be("percent");
    }

    [Fact]
    public async Task GetMetricMetadataAsync_ReturnsError_WhenMetricNotFound()
    {
        // Arrange
        _mockClient.GetMetricMetadataAsync("nonexistent.metric", Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Not found"));

        // Act
        var result = await _tool.GetMetricMetadataAsync("nonexistent.metric");

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    public async Task QueryMetricsAsync_ParsesRelativeTime_Correctly()
    {
        // Arrange
        var response = new MetricQueryResponse { Series = new List<MetricSeries>() };

        DateTime capturedFrom = default;
        DateTime capturedTo = default;

        _mockClient.QueryMetricsAsync(
            Arg.Any<string>(),
            Arg.Do<DateTime>(d => capturedFrom = d),
            Arg.Do<DateTime>(d => capturedTo = d),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        // Act
        await _tool.QueryMetricsAsync("avg:system.cpu.user{*}", from: "1d", to: "now");

        // Assert
        capturedFrom.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));
        capturedTo.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }
}
