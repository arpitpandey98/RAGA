using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using RAGA.Application.Common.Extensions;
using RAGA.Application.DTOs;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Infrastructure.Services
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly RAGADbContext _dbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ITextExtractor _textExtractor;
        private readonly IChunker _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly ISearchIndexService _searchIndexService;

        public DocumentProcessingService(
            RAGADbContext dbContext,
            IFileStorageService fileStorageService,
            ITextExtractor textExtractor,
            IChunker chunker,
            IEmbeddingService embeddingService,
            ISearchIndexService searchIndexService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
            _textExtractor = textExtractor;
            _chunker = chunker;
            _embeddingService = embeddingService;
            _searchIndexService = searchIndexService;
        }

        public async Task ProcessDocumentAsync(int documentId,  CancellationToken ct)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(x => x.Id == documentId, ct);

            if (document == null)
            {
                throw new InvalidOperationException(
                    $"Document with ID {documentId} was not found.");
            }

            var existingChunks = await _dbContext.DocumentChunks
                .Where(x => x.DocumentId == document.Id)
                .ToListAsync(ct);

            _dbContext.DocumentChunks.RemoveRange(existingChunks);

            await using var fileStream =
                await _fileStorageService.GetFileAsync(
                    document.BlobPath,
                    ct);

            var pages = await _textExtractor.ExtractTextAsync(
                fileStream,
                ExtensionMapper.MapFileExtensionToFileType(document.FileType.ToString()),
                ct);

            var chunks = _chunker.ChunkPages(pages);

            var searchChunks = new List<SearchChunk>();

            foreach (var chunk in chunks)
            {
                ct.ThrowIfCancellationRequested();

                var embedding =
                    await _embeddingService.GenerateEmbeddingAsync(
                        chunk.Content,
                        ct);

                var documentChunk = new DocumentChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = chunk.ChunkIndex,
                    PageNumber = chunk.PageNumber,
                    Content = chunk.Content,
                    Embedding = new SqlVector<float>(embedding)
                };

                _dbContext.DocumentChunks.Add(documentChunk);

                searchChunks.Add(new SearchChunk
                {
                    ChunkIndex = chunk.ChunkIndex,
                    PageNumber = chunk.PageNumber,
                    Content = chunk.Content,
                    Embedding = embedding
                });
            }

            await _dbContext.SaveChangesAsync(ct);

            await _searchIndexService.EnsureIndexAsync(ct);

            await _searchIndexService.IndexChunksAsync(
                document.Id,
                document.FileName,
                searchChunks,
                ct);
        }
    }
}
