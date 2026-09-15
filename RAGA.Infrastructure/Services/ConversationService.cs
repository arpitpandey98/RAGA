using RAGA.Application.Interfaces;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RAGA.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly RAGADbContext _context;

    public ConversationService(RAGADbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateConversationAsync(
        CancellationToken ct)
    {
        var conversation = new Conversation();

        _context.Conversations.Add(conversation);

        await _context.SaveChangesAsync(ct);

        return conversation.Id;
    }

    public async Task AddMessageAsync(
        int conversationId,
        string role,
        string content,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException(
                "Message role cannot be empty.",
                nameof(role));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(content));
        }

        // make sure Microsoft.EntityFrameworkCore is imported so the IQueryable overload of AnyAsync is used
        var conversationExists =
            await _context.Conversations
                .AnyAsync(x => x.Id == conversationId, ct);

        if (!conversationExists)
        {
            throw new InvalidOperationException(
                $"Conversation {conversationId} does not exist.");
        }

        var message = new Message
        {
            ConversationId = conversationId,
            Role = role,
            Content = content
        };

        _context.Messages.Add(message);

        // pass keys as an object[] when providing a CancellationToken
        var conversation =
            await _context.Conversations
                .FindAsync(new object?[] { conversationId }, ct);

        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}