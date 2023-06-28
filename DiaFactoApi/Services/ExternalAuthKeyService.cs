using System.Collections.Concurrent;
using System.Security.Cryptography;
using DiaFactoApi.Models.Internal;

namespace DiaFactoApi.Services;

public class ExternalAuthKeyService
{
    private readonly ILogger<ExternalAuthKeyService> _logger;
    private readonly ConcurrentDictionary<string, ExternalAuthKey> _keys = new();
    public ExternalAuthKeyService(ILogger<ExternalAuthKeyService> logger) => _logger = logger;
    private static readonly TimeSpan KeyLifetime = TimeSpan.FromMinutes(5);

    private void RemoveExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (key, externalKey) in _keys)
        {
            if (now - externalKey.CreatedAt <= KeyLifetime) continue;
            _keys.TryRemove(key, out _);
            _logger.LogInformation("Removed expired key {key} for user {userId}", key, externalKey.UserId);
        }
    }

    public (string, DateTimeOffset) GenerateKey(int userId)
    {
        RemoveExpired();
        if (_keys.Values.Any(k => k.UserId == userId))
        {
            var oldKey = _keys.First(k => k.Value.UserId == userId).Key;
            _keys.TryRemove(oldKey, out _);
            _logger.LogInformation("Removed old key {key} for user {userId}", oldKey, userId);
        }

        var random = Guid.NewGuid().ToString()[..8];
        var key = $"{userId}{random}";
        var externalKey = new ExternalAuthKey
        {
            Key = key,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _keys.TryAdd(key, externalKey);
        _logger.LogInformation("Generated key {key} for user {userId}", key, userId);
        return (key, externalKey.CreatedAt + KeyLifetime);
    }

    public bool TryTakeUser(string key, int expectedUserId)
    {
        RemoveExpired();
        if (!_keys.TryGetValue(key, out var externalKey) || externalKey.UserId != expectedUserId)
            return false;
        _keys.TryRemove(key, out _);
        _logger.LogInformation("Removed key {key} for user {userId}", key, expectedUserId);
        return true;
    }
}