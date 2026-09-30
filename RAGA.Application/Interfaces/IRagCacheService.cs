namespace RAGA.Application.Interfaces;

public interface IRagCacheService
{
    Task<string?> GetAsync(
        int tenantId,
        string question,
        CancellationToken ct);

    Task SetAsync(
        int tenantId,
        string question,
        string response,
        CancellationToken ct);

    Task RemoveAsync(
        int tenantId,
        string question,
        CancellationToken ct);
}