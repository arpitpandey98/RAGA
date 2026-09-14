using RAGA.Application.DTOs;

namespace RAGA.Infrastructure.Interfaces
{
    public interface ISearchIndexService
    {
        Task IndexChunksAsync(
        int documentId,
        string title,
        List<SearchChunk> chunks,
        CancellationToken ct);

        Task<List<SearchResult>> SearchAsync(
            string query,
            int topK,
            CancellationToken ct);

        Task EnsureIndexAsync(
        CancellationToken ct);

        Task DeleteDocumentAsync(
        int documentId,
        CancellationToken ct);
    }
}
