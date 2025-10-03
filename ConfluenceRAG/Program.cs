using ConfluenceRAG.Data.Models.Config;
using ConfluenceRAG.Data.Services;
using Microsoft.Extensions.Configuration;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var appConfig = config.Get<AppConfig>();

Console.WriteLine("Confluence RAG Demo");

// 1) Fetch pages
Console.WriteLine("Fetching Confluence pages...");
var confluenceService = new ConfluenceService(appConfig.ConfluenceOrg);
var pages = await confluenceService.GetPages();
Console.WriteLine($"Retrieved {pages.Count} pages");

// 2) Index pages
var rag = new EmbeddingSearchService(appConfig);

Console.Write("Reindex pages? (y/n): ");
var reindexInput = Console.ReadLine();

if (reindexInput?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
{
    Console.WriteLine("Indexing pages...");
    await rag.RebuildIndex(pages);
    Console.WriteLine("Indexed pages");
}
else
{
    Console.WriteLine("Skipping reindex.");
}

// 3) Initialize chat service
var chatService = new ChatCompletionService(appConfig.AzureOpenAI);

// 4) Interactive chat
Console.WriteLine("Ready. Type 'quit' to exit.");
while (true)
{
    Console.Write("Question: ");
    var question = Console.ReadLine();

    if (
        string.IsNullOrWhiteSpace(question)
        || question.Equals("quit", StringComparison.OrdinalIgnoreCase)
    )
    {
        break;
    }

    Console.WriteLine("Searching...");

    var ragResults = new List<(string Title, string Content)>();
    await foreach (var hit in rag.SearchAsync(question, top: 3))
    {
        ragResults.Add((hit.Record.Name, hit.Record.Description));
        Console.WriteLine($"- {hit.Record.Name} [score {hit.Score:F3}]");
    }

    if (ragResults.Count == 0)
    {
        Console.WriteLine("No results.");
        continue;
    }

    Console.WriteLine("Answer:");

    await chatService.GetAnswerWithContextAsync(question, ragResults);
}