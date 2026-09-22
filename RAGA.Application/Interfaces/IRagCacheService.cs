namespace RAGA.Application.Interfaces;

public interface IRagCacheService
{
    Task<string?> GetAsync(
        string question,
        CancellationToken ct);

    Task SetAsync(
        string question,
        string response,
        CancellationToken ct);

    Task RemoveAsync(
        string question,
        CancellationToken ct);
}