namespace RAGA.Application.Interfaces;

public interface IConversationService
{
    Task<int> CreateConversationAsync(
        CancellationToken ct);

    Task AddMessageAsync(
        int conversationId,
        string role,
        string content,
        CancellationToken ct);
}