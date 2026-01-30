using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using Viamus.DataDog.Mcp.Server.Models;
using Viamus.DataDog.Mcp.Server.Services;
using Viamus.DataDog.Mcp.Server.Tools;

namespace Viamus.DataDog.Mcp.Server.Tests.Tools;

public class DashboardsToolTests
{
    private readonly IDatadogClient _mockClient;
    private readonly ILogger<DashboardsTool> _mockLogger;
    private readonly DashboardsTool _tool;

    public DashboardsToolTests()
    {
        _mockClient = Substitute.For<IDatadogClient>();
        _mockLogger = Substitute.For<ILogger<DashboardsTool>>();
        _tool = new DashboardsTool(_mockClient, _mockLogger);
    }

    [Fact]
    public async Task ListDashboardsAsync_ReturnsDashboards_WhenDashboardsExist()
    {
        // Arrange
        var response = new DashboardListResponse
        {
            Dashboards = new List<DashboardSummary>
            {
                new()
                {
                    Id = "dash-123",
                    Title = "Production Overview",
                    Description = "Main production metrics",
                    LayoutType = "ordered",
                    Url = "/dashboard/dash-123",
                    IsReadOnly = false,
                    AuthorHandle = "john@example.com",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    ModifiedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = "dash-456",
                    Title = "API Performance",
                    Description = "API latency and throughput",
                    LayoutType = "free",
                    Url = "/dashboard/dash-456",
                    IsReadOnly = true,
                    AuthorHandle = "jane@example.com"
                }
            }
        };

        _mockClient.GetDashboardsAsync(Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _tool.ListDashboardsAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("dashboards").EnumerateArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task ListDashboardsAsync_ReturnsEmptyList_WhenNoDashboardsExist()
    {
        // Arrange
        var response = new DashboardListResponse
        {
            Dashboards = new List<DashboardSummary>()
        };

        _mockClient.GetDashboardsAsync(Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _tool.ListDashboardsAsync();

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ListDashboardsAsync_ReturnsError_WhenHttpRequestFails()
    {
        // Arrange
        _mockClient.GetDashboardsAsync(Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("API error"));

        // Act
        var result = await _tool.ListDashboardsAsync();

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetString().Should().Contain("Failed to list dashboards");
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsDashboardDetails_WhenDashboardExists()
    {
        // Arrange
        var dashboard = new Dashboard
        {
            Id = "dash-123",
            Title = "Production Overview",
            Description = "Main production metrics",
            LayoutType = "ordered",
            Url = "/dashboard/dash-123",
            IsReadOnly = false,
            AuthorHandle = "john@example.com",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            ModifiedAt = DateTime.UtcNow.AddDays(-1),
            ReflowType = "fixed",
            NotifyList = new List<string> { "@oncall" },
            TemplateVariables = new List<TemplateVariable>
            {
                new()
                {
                    Name = "env",
                    Prefix = "environment",
                    Default = "prod",
                    AvailableValues = new List<string> { "prod", "staging", "dev" }
                }
            },
            Widgets = new List<DashboardWidget>
            {
                new()
                {
                    Id = 1,
                    Definition = new WidgetDefinition
                    {
                        Type = "timeseries",
                        Title = "CPU Usage",
                        Requests = new List<WidgetRequest>
                        {
                            new() { Query = "avg:system.cpu.user{*}" }
                        }
                    },
                    Layout = new WidgetLayout { X = 0, Y = 0, Width = 6, Height = 4 }
                },
                new()
                {
                    Id = 2,
                    Definition = new WidgetDefinition
                    {
                        Type = "timeseries",
                        Title = "Memory Usage",
                        Requests = new List<WidgetRequest>
                        {
                            new() { Query = "avg:system.mem.used{*}" }
                        }
                    },
                    Layout = new WidgetLayout { X = 6, Y = 0, Width = 6, Height = 4 }
                }
            }
        };

        _mockClient.GetDashboardAsync("dash-123", Arg.Any<CancellationToken>())
            .Returns(dashboard);

        // Act
        var result = await _tool.GetDashboardAsync("dash-123");

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("id").GetString().Should().Be("dash-123");
        json.RootElement.GetProperty("title").GetString().Should().Be("Production Overview");
        json.RootElement.GetProperty("widgetCount").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("widgets").EnumerateArray().Should().HaveCount(2);
        json.RootElement.GetProperty("templateVariables").EnumerateArray().Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsError_WhenDashboardNotFound()
    {
        // Arrange
        _mockClient.GetDashboardAsync("nonexistent", Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Not found"));

        // Act
        var result = await _tool.GetDashboardAsync("nonexistent");

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetDashboardAsync_HandlesNullWidgets_Gracefully()
    {
        // Arrange
        var dashboard = new Dashboard
        {
            Id = "dash-123",
            Title = "Empty Dashboard",
            Widgets = null
        };

        _mockClient.GetDashboardAsync("dash-123", Arg.Any<CancellationToken>())
            .Returns(dashboard);

        // Act
        var result = await _tool.GetDashboardAsync("dash-123");

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("widgetCount").GetInt32().Should().Be(0);
    }
}
