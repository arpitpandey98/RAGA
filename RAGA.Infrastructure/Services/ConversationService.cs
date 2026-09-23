using Microsoft.EntityFrameworkCore;
using RAGA.Application.Interfaces;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using System.Text.Json;

namespace RAGA.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly RAGADbContext _context;
    private readonly IConversationCacheService _conversationCache;

    public ConversationService(RAGADbContext context, IConversationCacheService conversationCache)
    {
        _context = context;
        _conversationCache = conversationCache;
    }

    public async Task<int> CreateConversationAsync(
    string userId,
    CancellationToken ct)
    {
        var conversation = new Conversation
        {
            UserId = userId
        };

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
        if (role is not ("user" or "assistant"))
        {
            throw new ArgumentException(
                "Message role must be either 'user' or 'assistant'.",
                nameof(role));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Message content cannot be empty.",
                nameof(content));
        }

        if (role == "user" && content.Length > 4000)
        {
            throw new ArgumentException(
                "User message cannot exceed 4000 characters.",
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

        await _conversationCache.RemoveAsync(
        conversationId,
        ct);
    }

    public async Task<bool> BelongsToUserAsync(
    int conversationId,
    string userId,
    CancellationToken ct)
    {
        return await _context.Conversations
            .AnyAsync(
                x => x.Id == conversationId &&
                     x.UserId == userId,
                ct);
    }

    public async Task<List<Message>> GetMessagesAsync(
    int conversationId,
    string userId,
    CancellationToken ct)
    {
        var cached = await _conversationCache.GetAsync(
            conversationId,
            ct);

        if (cached != null)
        {
            return JsonSerializer.Deserialize<List<Message>>(cached)
                   ?? [];
        }

        var messages = await _context.Messages
            .Where(x =>
                x.ConversationId == conversationId &&
                x.Conversation.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        var serialized = JsonSerializer.Serialize(messages);

        await _conversationCache.SetAsync(
            conversationId,
            serialized,
            ct);

        return messages;
    }
}