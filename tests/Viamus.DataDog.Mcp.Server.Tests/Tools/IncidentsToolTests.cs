using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using Viamus.DataDog.Mcp.Server.Models;
using Viamus.DataDog.Mcp.Server.Services;
using Viamus.DataDog.Mcp.Server.Tools;

namespace Viamus.DataDog.Mcp.Server.Tests.Tools;

public class IncidentsToolTests
{
    private readonly IDatadogClient _mockClient;
    private readonly ILogger<IncidentsTool> _mockLogger;
    private readonly IncidentsTool _tool;

    public IncidentsToolTests()
    {
        _mockClient = Substitute.For<IDatadogClient>();
        _mockLogger = Substitute.For<ILogger<IncidentsTool>>();
        _tool = new IncidentsTool(_mockClient, _mockLogger);
    }

    [Fact]
    public async Task ListIncidentsAsync_ReturnsIncidents_WhenIncidentsExist()
    {
        // Arrange
        var response = new IncidentListResponse
        {
            Data = new List<Incident>
            {
                new()
                {
                    Id = "inc-123",
                    Type = "incidents",
                    Attributes = new IncidentAttributes
                    {
                        Title = "Database outage",
                        Severity = "SEV-1",
                        State = "active",
                        CustomerImpacted = true,
                        CustomerImpactScope = "All users affected",
                        Created = DateTime.UtcNow.AddHours(-2),
                        Detected = DateTime.UtcNow.AddHours(-2),
                        PublicId = 123
                    }
                },
                new()
                {
                    Id = "inc-456",
                    Type = "incidents",
                    Attributes = new IncidentAttributes
                    {
                        Title = "API latency spike",
                        Severity = "SEV-2",
                        State = "resolved",
                        CustomerImpacted = false,
                        Created = DateTime.UtcNow.AddDays(-1),
                        Resolved = DateTime.UtcNow.AddHours(-20),
                        PublicId = 456
                    }
                }
            },
            Meta = new IncidentMeta
            {
                Pagination = new IncidentPagination
                {
                    Offset = 0,
                    Size = 25
                }
            }
        };

        _mockClient.GetIncidentsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _tool.ListIncidentsAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("incidents").EnumerateArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task ListIncidentsAsync_PassesPagination_Correctly()
    {
        // Arrange
        int capturedPageSize = 0;
        int capturedPageOffset = 0;

        _mockClient.GetIncidentsAsync(
            Arg.Do<int>(p => capturedPageSize = p),
            Arg.Do<int>(o => capturedPageOffset = o),
            Arg.Any<CancellationToken>()
        ).Returns(new IncidentListResponse { Data = new List<Incident>() });

        // Act
        await _tool.ListIncidentsAsync(pageSize: 50, pageOffset: 100);

        // Assert
        capturedPageSize.Should().Be(50);
        capturedPageOffset.Should().Be(100);
    }

    [Fact]
    public async Task ListIncidentsAsync_ClampsPageSize_ToValidRange()
    {
        // Arrange
        int capturedPageSize = 0;

        _mockClient.GetIncidentsAsync(
            Arg.Do<int>(p => capturedPageSize = p),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()
        ).Returns(new IncidentListResponse { Data = new List<Incident>() });

        // Act
        await _tool.ListIncidentsAsync(pageSize: 500);

        // Assert
        capturedPageSize.Should().Be(100); // Clamped to max
    }

    [Fact]
    public async Task ListIncidentsAsync_ReturnsError_WhenHttpRequestFails()
    {
        // Arrange
        _mockClient.GetIncidentsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("API error"));

        // Act
        var result = await _tool.ListIncidentsAsync();

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetString().Should().Contain("Failed to list incidents");
    }

    [Fact]
    public async Task GetIncidentAsync_ReturnsIncidentDetails_WhenIncidentExists()
    {
        // Arrange
        var incident = new Incident
        {
            Id = "inc-123",
            Type = "incidents",
            Attributes = new IncidentAttributes
            {
                Title = "Database outage",
                Severity = "SEV-1",
                State = "active",
                CustomerImpacted = true,
                CustomerImpactScope = "All users affected",
                CustomerImpactStart = DateTime.UtcNow.AddHours(-2),
                Created = DateTime.UtcNow.AddHours(-2),
                Modified = DateTime.UtcNow,
                Detected = DateTime.UtcNow.AddHours(-2),
                TimeToDetect = 300,
                TimeToInternalResponse = 600,
                PublicId = 123,
                NotificationHandles = new List<string> { "@oncall", "@slack-alerts" }
            },
            Relationships = new IncidentRelationships
            {
                CommanderUser = new IncidentRelationshipData
                {
                    Data = new IncidentRelationshipItem { Id = "user-1", Type = "users" }
                }
            }
        };

        _mockClient.GetIncidentAsync("inc-123", Arg.Any<CancellationToken>())
            .Returns(incident);

        // Act
        var result = await _tool.GetIncidentAsync("inc-123");

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("id").GetString().Should().Be("inc-123");
        json.RootElement.GetProperty("title").GetString().Should().Be("Database outage");
        json.RootElement.GetProperty("severity").GetString().Should().Be("SEV-1");
        json.RootElement.GetProperty("customerImpacted").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetIncidentAsync_ReturnsError_WhenIncidentNotFound()
    {
        // Arrange
        _mockClient.GetIncidentAsync("nonexistent", Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Not found"));

        // Act
        var result = await _tool.GetIncidentAsync("nonexistent");

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }
}
