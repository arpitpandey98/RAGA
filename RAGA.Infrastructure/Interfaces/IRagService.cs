using RAGA.Application.DTOs;
using System.Security.Claims;

namespace RAGA.Application.Interfaces;

public interface IRagService
{
    Task<RagResponse> AskAsync(
        string question,
        ClaimsPrincipal user,
        CancellationToken ct);
}