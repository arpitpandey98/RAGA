using RAGA.Domain.Entities;

namespace RAGA.Application.Interfaces;

public interface IConversationService
{
    Task<int> CreateConversationAsync(
    string userId,
    CancellationToken ct);

    Task AddMessageAsync(
        int conversationId,
        string role,
        string content,
        CancellationToken ct);

    Task<bool> BelongsToUserAsync(
    int conversationId,
    string userId,
    CancellationToken ct);

    Task<List<Message>> GetMessagesAsync(
    int conversationId,
    string userId,
    CancellationToken ct);

}