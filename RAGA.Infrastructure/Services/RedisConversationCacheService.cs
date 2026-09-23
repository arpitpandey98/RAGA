using Microsoft.Extensions.Caching.Distributed;
using RAGA.Application.Interfaces;

namespace RAGA.Infrastructure.Services;

public class RedisConversationCacheService : IConversationCacheService
{
    private readonly IDistributedCache _cache;

    public RedisConversationCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetAsync(
        int conversationId,
        CancellationToken ct)
    {
        var key = GetCacheKey(conversationId);

        return await _cache.GetStringAsync(key, ct);
    }

    public async Task SetAsync(
        int conversationId,
        string value,
        CancellationToken ct)
    {
        var key = GetCacheKey(conversationId);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync(
            key,
            value,
            options,
            ct);
    }

    public async Task RemoveAsync(
        int conversationId,
        CancellationToken ct)
    {
        var key = GetCacheKey(conversationId);

        await _cache.RemoveAsync(key, ct);
    }

    private static string GetCacheKey(int conversationId)
    {
        return $"raga:conversation:{conversationId}";
    }
}