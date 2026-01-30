using System.Text.Json;
using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Tests.Models;

public class DashboardModelsTests
{
    [Fact]
    public void DashboardListResponse_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "dashboards": [
                {
                    "id": "dash-123",
                    "title": "Production Overview",
                    "description": "Main production metrics",
                    "layout_type": "ordered",
                    "url": "/dashboard/dash-123",
                    "is_read_only": false,
                    "created_at": "2024-01-01T00:00:00Z",
                    "modified_at": "2024-01-15T10:00:00Z",
                    "author_handle": "john@example.com"
                },
                {
                    "id": "dash-456",
                    "title": "API Metrics",
                    "layout_type": "free",
                    "is_read_only": true
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<DashboardListResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Dashboards.Should().HaveCount(2);
        result.Dashboards![0].Id.Should().Be("dash-123");
        result.Dashboards[0].Title.Should().Be("Production Overview");
        result.Dashboards[0].LayoutType.Should().Be("ordered");
        result.Dashboards[0].IsReadOnly.Should().BeFalse();
        result.Dashboards[1].IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void Dashboard_WithWidgets_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "id": "dash-789",
            "title": "Full Dashboard",
            "layout_type": "ordered",
            "template_variables": [
                {
                    "name": "env",
                    "prefix": "environment",
                    "default": "prod",
                    "available_values": ["prod", "staging", "dev"]
                }
            ],
            "widgets": [
                {
                    "id": 1,
                    "definition": {
                        "type": "timeseries",
                        "title": "CPU Usage",
                        "title_size": "16",
                        "title_align": "left",
                        "requests": [
                            {
                                "q": "avg:system.cpu.user{$env}",
                                "display_type": "line",
                                "style": {
                                    "palette": "dog_classic",
                                    "line_type": "solid",
                                    "line_width": "normal"
                                }
                            }
                        ]
                    },
                    "layout": {
                        "x": 0,
                        "y": 0,
                        "width": 6,
                        "height": 4
                    }
                }
            ],
            "notify_list": ["@oncall", "@slack-alerts"],
            "reflow_type": "fixed"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Dashboard>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("dash-789");
        result.TemplateVariables.Should().HaveCount(1);
        result.TemplateVariables![0].Name.Should().Be("env");
        result.TemplateVariables[0].AvailableValues.Should().Contain("prod");
        result.Widgets.Should().HaveCount(1);
        result.Widgets![0].Definition!.Type.Should().Be("timeseries");
        result.Widgets[0].Definition.Title.Should().Be("CPU Usage");
        result.Widgets[0].Layout!.Width.Should().Be(6);
        result.NotifyList.Should().Contain("@oncall");
    }

    [Fact]
    public void DashboardWidget_WithStyle_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "id": 42,
            "definition": {
                "type": "timeseries",
                "title": "Memory",
                "requests": [
                    {
                        "q": "avg:system.mem.used{*}",
                        "display_type": "area",
                        "style": {
                            "palette": "warm",
                            "line_type": "dashed",
                            "line_width": "thick"
                        }
                    }
                ]
            },
            "layout": {
                "x": 6,
                "y": 0,
                "width": 6,
                "height": 4
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<DashboardWidget>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Definition!.Requests.Should().HaveCount(1);
        result.Definition.Requests![0].Style!.Palette.Should().Be("warm");
        result.Definition.Requests[0].Style.LineType.Should().Be("dashed");
        result.Definition.Requests[0].DisplayType.Should().Be("area");
    }

    [Fact]
    public void WidgetLayout_Serializes_Correctly()
    {
        // Arrange
        var layout = new WidgetLayout
        {
            X = 0,
            Y = 4,
            Width = 12,
            Height = 6
        };

        // Act
        var json = JsonSerializer.Serialize(layout);
        var parsed = JsonDocument.Parse(json);

        // Assert
        parsed.RootElement.GetProperty("x").GetInt32().Should().Be(0);
        parsed.RootElement.GetProperty("y").GetInt32().Should().Be(4);
        parsed.RootElement.GetProperty("width").GetInt32().Should().Be(12);
        parsed.RootElement.GetProperty("height").GetInt32().Should().Be(6);
    }
}
