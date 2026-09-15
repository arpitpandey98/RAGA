using RAGA.Application.DTOs;

namespace RAGA.Application.Interfaces;

public interface IRagService
{
    Task<RagResponse> AskAsync(
        string question,
        CancellationToken ct);
}