using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using RAGA.Application.Interfaces;

namespace RAGA.Infrastructure.Services;

public class RedisRagCacheService : IRagCacheService
{
    private readonly IDistributedCache _cache;

    public RedisRagCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetAsync(
        string question,
        CancellationToken ct)
    {
        var key = CreateCacheKey(question);

        return await _cache.GetStringAsync(key, ct);
    }

    public async Task SetAsync(
        string question,
        string response,
        CancellationToken ct)
    {
        var key = CreateCacheKey(question);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync(
            key,
            response,
            options,
            ct);
    }

    public async Task RemoveAsync(
        string question,
        CancellationToken ct)
    {
        var key = CreateCacheKey(question);

        await _cache.RemoveAsync(key, ct);
    }

    private static string CreateCacheKey(string question)
    {
        var normalizedQuestion =
            question.Trim().ToLowerInvariant();

        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(normalizedQuestion));

        var hashString =
            Convert.ToHexString(hash);

        return $"raga:rag:{hashString}";
    }
}