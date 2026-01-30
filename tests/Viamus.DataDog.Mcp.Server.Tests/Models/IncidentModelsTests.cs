using System.Text.Json;
using Viamus.DataDog.Mcp.Server.Models;

namespace Viamus.DataDog.Mcp.Server.Tests.Models;

public class IncidentModelsTests
{
    [Fact]
    public void IncidentListResponse_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "data": [
                {
                    "id": "inc-123",
                    "type": "incidents",
                    "attributes": {
                        "title": "Database outage",
                        "customer_impact_scope": "All users affected",
                        "customer_impact_start": "2024-01-15T10:00:00Z",
                        "customer_impacted": true,
                        "created": "2024-01-15T10:05:00Z",
                        "modified": "2024-01-15T12:00:00Z",
                        "detected": "2024-01-15T10:00:00Z",
                        "severity": "SEV-1",
                        "state": "active",
                        "time_to_detect": 300,
                        "public_id": 123
                    }
                }
            ],
            "meta": {
                "pagination": {
                    "offset": 0,
                    "size": 25,
                    "next_offset": 25
                }
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<IncidentListResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);
        result.Data![0].Id.Should().Be("inc-123");
        result.Data[0].Attributes!.Title.Should().Be("Database outage");
        result.Data[0].Attributes.Severity.Should().Be("SEV-1");
        result.Data[0].Attributes.State.Should().Be("active");
        result.Data[0].Attributes.CustomerImpacted.Should().BeTrue();
        result.Data[0].Attributes.TimeToDetect.Should().Be(300);
        result.Meta!.Pagination!.NextOffset.Should().Be(25);
    }

    [Fact]
    public void Incident_WithRelationships_Deserializes_Correctly()
    {
        // Arrange
        var json = """
        {
            "id": "inc-456",
            "type": "incidents",
            "attributes": {
                "title": "API latency spike",
                "severity": "SEV-2",
                "state": "resolved",
                "created": "2024-01-14T08:00:00Z",
                "resolved": "2024-01-14T10:00:00Z",
                "public_id": 456
            },
            "relationships": {
                "commander_user": {
                    "data": {
                        "id": "user-123",
                        "type": "users"
                    }
                },
                "created_by_user": {
                    "data": {
                        "id": "user-456",
                        "type": "users"
                    }
                }
            }
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<Incident>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Relationships.Should().NotBeNull();
        result.Relationships!.CommanderUser!.Data!.Id.Should().Be("user-123");
        result.Relationships.CreatedByUser!.Data!.Id.Should().Be("user-456");
    }

    [Fact]
    public void IncidentAttributes_HandlesNullFields_Gracefully()
    {
        // Arrange
        var json = """
        {
            "title": "Minimal incident",
            "created": "2024-01-15T10:00:00Z",
            "state": "active",
            "public_id": 789
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<IncidentAttributes>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Minimal incident");
        result.Severity.Should().BeNull();
        result.CustomerImpactScope.Should().BeNull();
        result.Resolved.Should().BeNull();
        result.TimeToResolve.Should().BeNull();
    }
}
