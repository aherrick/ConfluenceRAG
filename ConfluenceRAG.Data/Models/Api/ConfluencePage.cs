using System.Text.Json.Serialization;

namespace ConfluenceRAG.Data.Models.Api;

public static class ConfluencePage
{
    public sealed class Root
    {
        [JsonPropertyName("results")]
        public Page[] Results { get; set; } = [];

        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

    public sealed class Page
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("body")]
        public Body Body { get; set; }

        [JsonPropertyName("_links")]
        public Links Links { get; set; } = new Links();
    }

    public sealed class Body
    {
        [JsonPropertyName("storage")]
        public Storage Storage { get; set; }
    }

    public sealed class Storage
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}