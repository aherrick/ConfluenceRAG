using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using ConfluenceRAG.Data.Models.Config;
using ConfluenceRAG.Data.Models.Dto;
using ConfluenceRAG.Data.Models.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace ConfluenceRAG.Data.Services;

public class EmbeddingSearchService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private readonly VectorStore _vectorStore;
    private readonly VectorStoreCollection<string, ConfluencePageVector> _collection;
    private readonly string _embeddingModel;

    public EmbeddingSearchService(AppConfig config)
    {
        _embeddingModel = config.AzureOpenAI.EmbeddingDeployment;

        _generator = new AzureOpenAIClient(
            new Uri(config.AzureOpenAI.Endpoint),
            new AzureKeyCredential(config.AzureOpenAI.ApiKey)
        )
            .GetEmbeddingClient(_embeddingModel)
            .AsIEmbeddingGenerator();

        if (
            !string.IsNullOrWhiteSpace(config.AzureAISearch?.Endpoint)
            && !string.IsNullOrWhiteSpace(config.AzureAISearch?.ApiKey)
        )
        {
            var searchIndexClient = new SearchIndexClient(
                new Uri(config.AzureAISearch.Endpoint),
                new AzureKeyCredential(config.AzureAISearch.ApiKey)
            );
            _vectorStore = new AzureAISearchVectorStore(searchIndexClient);
        }
        else if (!string.IsNullOrWhiteSpace(config.SqliteVectorDBPath))
        {
            _vectorStore = new SqliteVectorStore($"Data Source={config.SqliteVectorDBPath}");
        }
        else
        {
            _vectorStore = new InMemoryVectorStore();
        }

        _collection = _vectorStore.GetCollection<string, ConfluencePageVector>(
            config.IndexName,
            BuildVectorDef() // manually build up the definition instead of properties on the model because of dims
        );
    }

    public async Task RebuildIndex(List<ConfluencePageDto> dtos)
    {
        await _collection.EnsureCollectionDeletedAsync();
        await _collection.EnsureCollectionExistsAsync();

        // build a flat list of all chunks to embed so we know the total
        var allChunks = new List<(string Title, string Content)>();
        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Body))
                continue;

            var chunks = EmbeddingChunker.ChunkWithTitle(dto.Title, dto.Body, _embeddingModel);
            allChunks.AddRange(chunks.Select(c => (dto.Title, c.content)));
        }

        int processed = 0;

        foreach (var (title, content) in allChunks)
        {
            processed++;

            var embedding = await _generator.GenerateAsync(content);

            var record = new ConfluencePageVector
            {
                Key = Guid.CreateVersion7().ToString(),
                Name = title,
                Description = content,
                Vector = embedding.Vector,
            };

            await _collection.UpsertAsync(record);

            Console.WriteLine($"{processed}/{allChunks.Count}");
        }

        Console.WriteLine("Indexing complete.");
    }

    public async IAsyncEnumerable<VectorSearchResult<ConfluencePageVector>> SearchAsync(
        string query,
        int top = 3
    )
    {
        await _collection.EnsureCollectionExistsAsync();
        var queryEmbedding = await _generator.GenerateVectorAsync(query ?? string.Empty);

        await foreach (var result in _collection.SearchAsync(queryEmbedding, top: top))
        {
            yield return result;
        }
    }

    // Only dims, kept simple
    private VectorStoreCollectionDefinition BuildVectorDef()
    {
        int dims = _embeddingModel.ToLowerInvariant() switch
        {
            "text-embedding-3-large" => 3072,
            _ => 1536,
        };

        // this maps to ConfluencePageVector properties
        return new VectorStoreCollectionDefinition
        {
            Properties =
            {
                new VectorStoreKeyProperty(nameof(ConfluencePageVector.Key), typeof(string)),
                new VectorStoreDataProperty(nameof(ConfluencePageVector.Name), typeof(string)),
                new VectorStoreDataProperty(
                    nameof(ConfluencePageVector.Description),
                    typeof(string)
                ),
                new VectorStoreVectorProperty(
                    nameof(ConfluencePageVector.Vector),
                    typeof(ReadOnlyMemory<float>),
                    dimensions: dims
                ),
            },
        };
    }
}