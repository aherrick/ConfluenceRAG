using System.Text.Json.Serialization;

namespace ConfluenceRAG.Data.Models.Api;

public static class ConfluenceSpace
{
    public sealed class Root
    {
        [JsonPropertyName("results")]
        public Space[] Results { get; set; } = [];

        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

    public sealed class Space
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}