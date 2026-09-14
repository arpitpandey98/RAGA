using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using RAGA.Application.DTOs;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Infrastructure.Services;

public class DocumentSearchService : IDocumentSearchService
{
    private readonly RAGADbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public DocumentSearchService(
        RAGADbContext dbContext,
        IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<List<DocumentSearchResult>> SearchAsync(
        string query,
        int topK = 3,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(query));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topK),
                "topK must be greater than zero.");
        }

        var queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                ct);

        var queryVector = new SqlVector<float>(queryEmbedding);

        var results = await _dbContext.DocumentChunks
            .AsNoTracking()
            .Select(chunk => new
            {
                Chunk = chunk,
                Distance = EF.Functions.VectorDistance(
                    "cosine",
                    chunk.Embedding,
                    queryVector)
            })
            .OrderBy(x => x.Distance)
            .Take(topK)
            .Select(x => new DocumentSearchResult
            {
                DocumentId = x.Chunk.DocumentId,
                ChunkId = x.Chunk.Id,
                ChunkIndex = x.Chunk.ChunkIndex,
                Content = x.Chunk.Content,
                Distance = x.Distance
            })
            .ToListAsync(ct);

        return results;
    }
}