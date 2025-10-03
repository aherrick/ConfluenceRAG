using System.Text.Json.Serialization;

namespace ConfluenceRAG.Data.Models.Api;

public sealed class Links
{
    [JsonPropertyName("webui")]
    public string Webui { get; set; }

    [JsonPropertyName("next")]
    public string Next { get; set; }
}