using System.Text.Json.Serialization;

namespace Viamus.DataDog.Mcp.Server.Models;

public class DashboardListResponse
{
    [JsonPropertyName("dashboards")]
    public List<DashboardSummary>? Dashboards { get; set; }
}

public class DashboardSummary
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("layout_type")]
    public string? LayoutType { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("is_read_only")]
    public bool IsReadOnly { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [JsonPropertyName("author_handle")]
    public string? AuthorHandle { get; set; }
}

public class Dashboard
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("layout_type")]
    public string? LayoutType { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("is_read_only")]
    public bool IsReadOnly { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [JsonPropertyName("author_handle")]
    public string? AuthorHandle { get; set; }

    [JsonPropertyName("widgets")]
    public List<DashboardWidget>? Widgets { get; set; }

    [JsonPropertyName("template_variables")]
    public List<TemplateVariable>? TemplateVariables { get; set; }

    [JsonPropertyName("notify_list")]
    public List<string>? NotifyList { get; set; }

    [JsonPropertyName("reflow_type")]
    public string? ReflowType { get; set; }
}

public class DashboardWidget
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("definition")]
    public WidgetDefinition? Definition { get; set; }

    [JsonPropertyName("layout")]
    public WidgetLayout? Layout { get; set; }
}

public class WidgetDefinition
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("title_size")]
    public string? TitleSize { get; set; }

    [JsonPropertyName("title_align")]
    public string? TitleAlign { get; set; }

    [JsonPropertyName("requests")]
    public List<WidgetRequest>? Requests { get; set; }
}

public class WidgetRequest
{
    [JsonPropertyName("q")]
    public string? Query { get; set; }

    [JsonPropertyName("display_type")]
    public string? DisplayType { get; set; }

    [JsonPropertyName("style")]
    public WidgetStyle? Style { get; set; }
}

public class WidgetStyle
{
    [JsonPropertyName("palette")]
    public string? Palette { get; set; }

    [JsonPropertyName("line_type")]
    public string? LineType { get; set; }

    [JsonPropertyName("line_width")]
    public string? LineWidth { get; set; }
}

public class WidgetLayout
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class TemplateVariable
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    [JsonPropertyName("default")]
    public string? Default { get; set; }

    [JsonPropertyName("available_values")]
    public List<string>? AvailableValues { get; set; }
}
