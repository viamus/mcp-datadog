using System.Text.Json;
using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Tests.Models;

public class MetricModelsTests
{
    [Fact]
    public void MetricQueryResponse_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "status": "ok",
            "res_type": "time_series",
            "from_date": 1705300000000,
            "to_date": 1705310000000,
            "query": "avg:system.cpu.user{*}",
            "series": [
                {
                    "metric": "system.cpu.user",
                    "display_name": "system.cpu.user",
                    "scope": "host:web-01",
                    "expression": "avg:system.cpu.user{host:web-01}",
                    "tag_set": ["host:web-01"],
                    "aggr": "avg",
                    "unit": [
                        {
                            "family": "percentage",
                            "scale_factor": 1.0,
                            "name": "percent",
                            "short_name": "%",
                            "plural": "percent"
                        }
                    ],
                    "pointlist": [
                        [1705300000000, 45.5],
                        [1705300060000, 46.2],
                        [1705300120000, 44.8]
                    ]
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<MetricQueryResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("ok");
        result.ResType.Should().Be("time_series");
        result.Query.Should().Be("avg:system.cpu.user{*}");
        result.Series.Should().HaveCount(1);
        result.Series![0].Metric.Should().Be("system.cpu.user");
        result.Series[0].Aggregation.Should().Be("avg");
        result.Series[0].TagSet.Should().Contain("host:web-01");
        result.Series[0].PointList.Should().HaveCount(3);
        result.Series[0].Unit![0].ShortName.Should().Be("%");
    }

    [Fact]
    public void MetricMetadata_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "type": "gauge",
            "description": "Percentage of CPU time spent in user space",
            "short_name": "cpu user",
            "unit": "percent",
            "per_unit": null,
            "statsd_interval": 10
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<MetricMetadata>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("gauge");
        result.Description.Should().Contain("CPU time");
        result.Unit.Should().Be("percent");
        result.StatsdInterval.Should().Be(10);
    }

    [Fact]
    public void MetricSeries_PointList_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "metric": "test.metric",
            "pointlist": [
                [1705300000000, 10.5],
                [1705300060000, 12.3],
                [1705300120000, 15.2]
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<MetricSeries>(json);

        // Assert
        result.Should().NotBeNull();
        result!.PointList.Should().HaveCount(3);
        result.PointList![0][0].Should().Be(1705300000000);
        result.PointList[0][1].Should().Be(10.5);
        result.PointList[1][1].Should().Be(12.3);
        result.PointList[2][1].Should().Be(15.2);
    }
}
