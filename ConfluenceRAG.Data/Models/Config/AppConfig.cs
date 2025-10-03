namespace ConfluenceRAG.Data.Models.Config;

public class AppConfig
{
    public string IndexName { get; set; }
    public string ConfluenceOrg { get; set; }

    public string SqliteVectorDBPath { get; set; }

    public AzureOpenAIConfig AzureOpenAI { get; set; }
    public AzureAISearchConfig AzureAISearch { get; set; }
}