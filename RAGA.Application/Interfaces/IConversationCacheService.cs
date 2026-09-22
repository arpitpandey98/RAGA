namespace RAGA.Application.Interfaces;

public interface IConversationCacheService
{
    Task<string?> GetAsync(
        int conversationId,
        CancellationToken ct);

    Task SetAsync(
        int conversationId,
        string value,
        CancellationToken ct);

    Task RemoveAsync(
        int conversationId,
        CancellationToken ct);
}