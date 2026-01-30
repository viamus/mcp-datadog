using Viamus.DataDog.Mcp.Server.Configuration;

namespace Viamus.DataDog.Mcp.Server.Tests.Configuration;

public class DatadogSettingsTests
{
    [Fact]
    public void SectionName_ReturnsCorrectValue()
    {
        // Assert
        DatadogSettings.SectionName.Should().Be("Datadog");
    }

    [Fact]
    public void BaseUrl_DefaultsSite_ToDatadoghqCom()
    {
        // Arrange
        var settings = new DatadogSettings
        {
            ApiKey = "test-api-key",
            ApplicationKey = "test-app-key"
        };

        // Assert
        settings.Site.Should().Be("datadoghq.com");
        settings.BaseUrl.Should().Be("https://api.datadoghq.com");
    }

    [Theory]
    [InlineData("datadoghq.com", "https://api.datadoghq.com")]
    [InlineData("datadoghq.eu", "https://api.datadoghq.eu")]
    [InlineData("us3.datadoghq.com", "https://api.us3.datadoghq.com")]
    [InlineData("us5.datadoghq.com", "https://api.us5.datadoghq.com")]
    [InlineData("ap1.datadoghq.com", "https://api.ap1.datadoghq.com")]
    public void BaseUrl_ConstructsCorrectly_ForDifferentSites(string site, string expectedBaseUrl)
    {
        // Arrange
        var settings = new DatadogSettings
        {
            ApiKey = "test-api-key",
            ApplicationKey = "test-app-key",
            Site = site
        };

        // Assert
        settings.BaseUrl.Should().Be(expectedBaseUrl);
    }

    [Fact]
    public void Settings_StoreApiKeys_Correctly()
    {
        // Arrange
        var settings = new DatadogSettings
        {
            ApiKey = "my-api-key-123",
            ApplicationKey = "my-app-key-456"
        };

        // Assert
        settings.ApiKey.Should().Be("my-api-key-123");
        settings.ApplicationKey.Should().Be("my-app-key-456");
    }
}
