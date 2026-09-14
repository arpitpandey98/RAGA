using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using RAGA.Application.Common.Extensions;
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

        public DocumentProcessingService(
            RAGADbContext dbContext,
            IFileStorageService fileStorageService,
            ITextExtractor textExtractor,
            IChunker chunker,
            IEmbeddingService embeddingService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
            _textExtractor = textExtractor;
            _chunker = chunker;
            _embeddingService = embeddingService;
        }

        public async Task ProcessDocumentAsync(
            int documentId,
            CancellationToken ct)
        {
            var document = await _dbContext.Documents
                .FirstOrDefaultAsync(x => x.Id == documentId, ct);

            if (document == null)
            {
                throw new InvalidOperationException(
                    $"Document with ID {documentId} was not found.");
            }

            await using var fileStream =
                await _fileStorageService.GetFileAsync(
                    document.BlobPath,
                    ct);

            var text = await _textExtractor.ExtractTextAsync(
                fileStream,
                ExtensionMapper.MapFileExtensionToFileType(document.FileType.ToString()),
                ct);

            var chunks = _chunker.ChunkText(text);

            foreach (var (chunk, index) in chunks.Select((value, index) => (value, index)))
            {
                ct.ThrowIfCancellationRequested();

                var embedding =
                    await _embeddingService.GenerateEmbeddingAsync(
                        chunk,
                        ct);

                var documentChunk = new DocumentChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = index,
                    Content = chunk,
                    Embedding = new SqlVector<float>(embedding)
                };

                _dbContext.DocumentChunks.Add(documentChunk);
            }

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
