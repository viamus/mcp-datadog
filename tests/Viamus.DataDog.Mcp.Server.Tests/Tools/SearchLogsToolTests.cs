using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using Viamus.DataDog.Mcp.Server.Models;
using Viamus.DataDog.Mcp.Server.Services;
using Viamus.DataDog.Mcp.Server.Tools;

namespace Viamus.DataDog.Mcp.Server.Tests.Tools;

public class SearchLogsToolTests
{
    private readonly IDatadogClient _mockClient;
    private readonly ILogger<SearchLogsTool> _mockLogger;
    private readonly SearchLogsTool _tool;

    public SearchLogsToolTests()
    {
        _mockClient = Substitute.For<IDatadogClient>();
        _mockLogger = Substitute.For<ILogger<SearchLogsTool>>();
        _tool = new SearchLogsTool(_mockClient, _mockLogger);
    }

    [Fact]
    public async Task SearchLogsAsync_ReturnsLogs_WhenQueryIsValid()
    {
        // Arrange
        var query = "service:api status:error";
        var response = new LogSearchResponse
        {
            Data = new List<LogEntry>
            {
                new()
                {
                    Id = "log-1",
                    Type = "log",
                    Attributes = new LogAttributes
                    {
                        Timestamp = DateTime.UtcNow,
                        Status = "error",
                        Service = "api",
                        Host = "host-1",
                        Message = "Test error message",
                        Tags = new List<string> { "env:prod" }
                    }
                }
            },
            Meta = new LogMeta
            {
                Status = "done",
                Elapsed = 100
            }
        };

        _mockClient.SearchLogsAsync(
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        // Act
        var result = await _tool.SearchLogsAsync(query);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(1);
        json.RootElement.GetProperty("logs").EnumerateArray().Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchLogsAsync_ReturnsEmptyList_WhenNoLogsFound()
    {
        // Arrange
        var query = "service:nonexistent";
        var response = new LogSearchResponse
        {
            Data = new List<LogEntry>(),
            Meta = new LogMeta { Status = "done" }
        };

        _mockClient.SearchLogsAsync(
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        // Act
        var result = await _tool.SearchLogsAsync(query);

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.GetProperty("total").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SearchLogsAsync_ReturnsError_WhenHttpRequestFails()
    {
        // Arrange
        var query = "service:api";
        _mockClient.SearchLogsAsync(
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Throws(new HttpRequestException("API error"));

        // Act
        var result = await _tool.SearchLogsAsync(query);

        // Assert
        var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetString().Should().Contain("Failed to search logs");
    }

    [Fact]
    public async Task SearchLogsAsync_ParsesRelativeTime_Correctly()
    {
        // Arrange
        var query = "service:api";
        var response = new LogSearchResponse { Data = new List<LogEntry>() };

        DateTime capturedFrom = default;
        DateTime capturedTo = default;

        _mockClient.SearchLogsAsync(
            Arg.Any<string>(),
            Arg.Do<DateTime>(d => capturedFrom = d),
            Arg.Do<DateTime>(d => capturedTo = d),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        // Act
        await _tool.SearchLogsAsync(query, from: "1h", to: "now");

        // Assert
        capturedFrom.Should().BeCloseTo(DateTime.UtcNow.AddHours(-1), TimeSpan.FromMinutes(1));
        capturedTo.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SearchLogsAsync_ClampsLimit_ToValidRange()
    {
        // Arrange
        var query = "service:api";
        var response = new LogSearchResponse { Data = new List<LogEntry>() };

        int capturedLimit = 0;

        _mockClient.SearchLogsAsync(
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Do<int>(l => capturedLimit = l),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(response);

        // Act
        await _tool.SearchLogsAsync(query, limit: 5000);

        // Assert
        capturedLimit.Should().Be(1000); // Clamped to max
    }
}
