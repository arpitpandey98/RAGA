namespace RAGA.Application.Interfaces;

public interface ITenantBootstrapService
{
    Task EnsureAdminTenantAsync(
        string ownerEntraObjectId,
        CancellationToken ct = default);
}