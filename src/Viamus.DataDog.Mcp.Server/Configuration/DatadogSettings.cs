namespace Viamus.DataDog.Mcp.Server.Configuration;

public class DatadogSettings
{
    public const string SectionName = "Datadog";

    public required string ApiKey { get; set; }
    public required string ApplicationKey { get; set; }
    public string Site { get; set; } = "api.datadoghq.com";

    public string BaseUrl => $"https://{Site}";
}
