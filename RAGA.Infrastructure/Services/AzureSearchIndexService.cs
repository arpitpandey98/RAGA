using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using RAGA.Application.DTOs;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Infrastructure.Services;

public class AzureSearchIndexService : ISearchIndexService
{
    private const int EmbeddingDimensions = 1536;

    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly IEmbeddingService _embeddingService;

    public AzureSearchIndexService(
        IConfiguration configuration,
        IEmbeddingService embeddingService)
    {
        var endpoint =
            configuration["AzureSearch:Endpoint"]
            ?? throw new InvalidOperationException(
                "AzureSearch:Endpoint is not configured.");

        var apiKey =
            configuration["AzureSearch:ApiKey"]
            ?? throw new InvalidOperationException(
                "AzureSearch:ApiKey is not configured.");

        var indexName =
            configuration["AzureSearch:IndexName"]
            ?? throw new InvalidOperationException(
                "AzureSearch:IndexName is not configured.");

        var credential = new AzureKeyCredential(apiKey);

        _indexClient = new SearchIndexClient(
            new Uri(endpoint),
            credential);

        _searchClient = new SearchClient(
            new Uri(endpoint),
            indexName,
            credential);

        _embeddingService = embeddingService;
    }

    public async Task EnsureIndexAsync(
        CancellationToken ct)
    {
        var indexName = _searchClient.IndexName;

        try
        {
            await _indexClient.GetIndexAsync(
                indexName,
                ct);

            return;
        }
        catch (RequestFailedException ex)
            when (ex.Status == 404)
        {
            // Index does not exist. Create it below.
        }

        var fields = new List<SearchField>
        {
            new SimpleField(
                "chunkId",
                SearchFieldDataType.String)
            {
                IsKey = true,
                IsFilterable = true
            },

            new SimpleField(
                "documentId",
                SearchFieldDataType.Int32)
            {
                IsFilterable = true
            },

            new SearchableField("content"),

            new SearchableField("title"),

            new SimpleField(
                "pageNumber",
                SearchFieldDataType.Int32)
            {
                IsFilterable = true
            },

            new SearchField(
                "embedding",
                SearchFieldDataType.Collection(
                    SearchFieldDataType.Single))
            {
                IsSearchable = true,
                VectorSearchDimensions = EmbeddingDimensions,
                VectorSearchProfileName = "hnsw-profile"
            },

            new SearchableField("metadata")
            {
                IsFilterable = true
            }
        };

        var vectorSearch = new VectorSearch
        {
            Algorithms =
            {
                new HnswAlgorithmConfiguration(
                    "hnsw-config")
            },

            Profiles =
            {
                new VectorSearchProfile(
                    "hnsw-profile",
                    "hnsw-config")
            }
        };

        var semanticConfiguration =
            new SemanticConfiguration(
                "default-semantic-config",
                new SemanticPrioritizedFields
                {
                    TitleField = new SemanticField("title"),

                    ContentFields =
                    {
                        new SemanticField("content")
                    }
                });

        var semanticSearch = new SemanticSearch
        {
            Configurations =
            {
                semanticConfiguration
            }
        };

        var index = new SearchIndex(indexName)
        {
            Fields = fields,
            VectorSearch = vectorSearch,
            SemanticSearch = semanticSearch
        };

        await _indexClient.CreateIndexAsync(
            index,
            ct);
    }

    public async Task IndexChunksAsync(
        int documentId,
        string title,
        List<SearchChunk> chunks,
        CancellationToken ct)
    {
        var documents = chunks.Select(chunk =>
            new SearchDocument
            {
                ["chunkId"] =
                    $"document-{documentId}-chunk-{chunk.ChunkIndex}",

                ["documentId"] = documentId,

                ["content"] = chunk.Content,

                ["title"] = title,

                ["pageNumber"] = chunk.PageNumber,

                ["embedding"] = chunk.Embedding,

                ["metadata"] =
                    $"documentId={documentId};page={chunk.PageNumber}"
            });

        await _searchClient.UploadDocumentsAsync(
            documents,
            cancellationToken: ct);
    }

    public async Task<List<SearchResult>> SearchAsync(
        string query,
        int topK,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Search query cannot be empty.",
                nameof(query));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topK));
        }

        var queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                ct);

        var candidateK = Math.Max(topK, 50);

        var vectorQuery =
            new VectorizedQuery(queryEmbedding)
            {
                KNearestNeighborsCount = candidateK,
                Fields =
                {
            "embedding"
                }
            };




        var options = new SearchOptions
        {
            Size = topK,

            QueryType = SearchQueryType.Semantic,

            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName =
            "default-semantic-config"
            },

            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    vectorQuery
                }
            }
        };

        var response =
            await _searchClient.SearchAsync<SearchDocument>(
                query,
                options,
                ct);

        var results = new List<SearchResult>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            var document = result.Document;

            results.Add(new SearchResult
            {
                DocumentId =
                    Convert.ToInt32(document["documentId"]),

                Title =
                    document["title"]?.ToString()
                    ?? string.Empty,

                Content =
                    document["content"]?.ToString()
                    ?? string.Empty,

                PageNumber =
                    Convert.ToInt32(document["pageNumber"]),

                Score = result.Score ?? 0,

                RerankerScore =  result.SemanticSearch?.RerankerScore
            });
        }

        return results;
    }

    public async Task DeleteDocumentAsync(
    int documentId,
    CancellationToken ct)
    {
        var options = new SearchOptions
        {
            Filter = $"documentId eq {documentId}",
            Size = 1000
        };

        options.Select.Add("chunkId");

        var response =
            await _searchClient.SearchAsync<SearchDocument>(
                "*",
                options,
                ct);

        var keys = new List<string>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            if (result.Document.TryGetValue(
                    "chunkId",
                    out var chunkId) &&
                chunkId is not null)
            {
                keys.Add(chunkId.ToString()!);
            }
        }

        if (keys.Count == 0)
        {
            return;
        }

        await _searchClient.DeleteDocumentsAsync(
            "chunkId",
            keys,
            cancellationToken: ct);
    }
}