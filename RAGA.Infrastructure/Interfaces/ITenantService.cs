using System.Security.Claims;
using RAGA.Application.DTOs;

namespace RAGA.Application.Interfaces;

public interface ITenantService
{
    Task<int> GetCurrentTenantIdAsync(
        ClaimsPrincipal user,
        CancellationToken ct);

    Task<CurrentTenantDto> GetCurrentTenantAsync(
        ClaimsPrincipal user,
        CancellationToken ct);
}