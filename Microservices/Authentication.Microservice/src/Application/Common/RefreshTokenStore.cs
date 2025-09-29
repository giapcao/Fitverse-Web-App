using System.Security.Cryptography;
using Application.Abstractions.Interface;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Common;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly IConnectionMultiplexer _mux;
    private readonly TimeSpan _ttl;
    private readonly int _dbIndex;

    public RefreshTokenStore(
        IConnectionMultiplexer mux,
        IOptions<Options.RefreshOptions> refreshOpt,
        IOptions<Options.RedisOptions> redisOpt)
    {
        _mux = mux;
        _ttl = TimeSpan.FromDays(refreshOpt.Value.ExpiryDays);
        _dbIndex = redisOpt.Value.Database;
    }

    public async Task<string> IssueAsync(Guid userId, CancellationToken ct)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var key = $"refresh:{userId}";
        var db = _mux.GetDatabase(_dbIndex);
        await db.StringSetAsync(key, token, _ttl);
        return token;
    }

    public async Task<(bool ok, string? current)> ValidateAsync(Guid userId, string incoming, CancellationToken ct)
    {
        var key = $"refresh:{userId}";
        var db = _mux.GetDatabase(_dbIndex);
        var val = await db.StringGetAsync(key);
        if (val.IsNullOrEmpty) return (false, null);
        return (val == incoming, val!);
    }

    public async Task RevokeAsync(Guid userId, CancellationToken ct)
    {
        var db = _mux.GetDatabase(_dbIndex);
        await db.KeyDeleteAsync($"refresh:{userId}");
    }
}