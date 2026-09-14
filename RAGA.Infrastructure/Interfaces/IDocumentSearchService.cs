using RAGA.Application.DTOs;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IDocumentSearchService
    {
        Task<List<DocumentSearchResult>> SearchAsync(
        string query,
        int topK = 3,
        CancellationToken ct = default);
    }
}
