using System.Security.Claims;

namespace RAGA.Application.Interfaces;

public interface ITenantService
{
    Task<int> GetCurrentTenantIdAsync(
        ClaimsPrincipal user,
        CancellationToken ct);
}