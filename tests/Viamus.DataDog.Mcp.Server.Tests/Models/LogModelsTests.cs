using System.Text.Json;
using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Tests.Models;

public class LogModelsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    [Fact]
    public void LogSearchResponse_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "data": [
                {
                    "id": "log-123",
                    "type": "log",
                    "attributes": {
                        "timestamp": "2024-01-15T10:30:00Z",
                        "status": "error",
                        "service": "api",
                        "host": "web-01",
                        "message": "Connection timeout",
                        "tags": ["env:prod", "team:backend"]
                    }
                }
            ],
            "meta": {
                "status": "done",
                "elapsed": 150,
                "page": {
                    "after": "cursor-abc"
                }
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<LogSearchResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);
        result.Data![0].Id.Should().Be("log-123");
        result.Data[0].Attributes!.Status.Should().Be("error");
        result.Data[0].Attributes.Service.Should().Be("api");
        result.Data[0].Attributes.Tags.Should().Contain("env:prod");
        result.Meta!.Status.Should().Be("done");
        result.Meta.Elapsed.Should().Be(150);
        result.Meta.Page!.After.Should().Be("cursor-abc");
    }

    [Fact]
    public void LogEntry_Serializes_Correctly()
    {
        // Arrange
        var entry = new LogEntry
        {
            Id = "log-456",
            Type = "log",
            Attributes = new LogAttributes
            {
                Timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                Status = "info",
                Service = "worker",
                Host = "worker-01",
                Message = "Job completed",
                Tags = new List<string> { "env:prod" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(entry);
        var parsed = JsonDocument.Parse(json);

        // Assert
        parsed.RootElement.GetProperty("id").GetString().Should().Be("log-456");
        parsed.RootElement.GetProperty("type").GetString().Should().Be("log");
    }

    [Fact]
    public void LogAttributes_HandlesNullFields_Gracefully()
    {
        // Arrange
        var json = """
        {
            "timestamp": "2024-01-15T10:30:00Z"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<LogAttributes>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().BeNull();
        result.Service.Should().BeNull();
        result.Tags.Should().BeNull();
    }
}
