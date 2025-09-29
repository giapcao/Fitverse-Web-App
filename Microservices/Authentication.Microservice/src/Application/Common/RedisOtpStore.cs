using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Interface;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Application.Common;

public sealed class RedisOtpStore : IOtpStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly OtpOptions _opt; 
    public RedisOtpStore(IConnectionMultiplexer redis, IOptions<OtpOptions> opt)
    {
        _redis = redis; _opt = opt.Value;
    }

    public async Task<bool> CanIssueAsync(string email, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var cooldownKey = $"pwd:otp:cooldown:{email}";
        if (await db.KeyExistsAsync(cooldownKey)) return false;

        var bucketKey = $"pwd:otp:sendcount:{email}:{DateTime.UtcNow:yyyyMMddHH}";
        var sends = await db.StringIncrementAsync(bucketKey);
        if (sends == 1) await db.KeyExpireAsync(bucketKey, TimeSpan.FromHours(1));
        if (sends > _opt.MaxSendsPerHour) return false;

        return true;
    }

    public async Task<string> IssueAsync(string email, Guid userId, TimeSpan ttl, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var otp = GenerateNumericOtp(_opt.Length);

        var payload = JsonSerializer.Serialize(new OtpPayload
        {
            Otp = otp,
            UserId = userId,
            Attempts = 0
        });

        var key = $"pwd:otp:{email}";
        await db.StringSetAsync(key, payload, ttl);

        var cooldownKey = $"pwd:otp:cooldown:{email}";
        await db.StringSetAsync(cooldownKey, "1", TimeSpan.FromSeconds(_opt.SendCooldownSeconds));

        return otp;
    }

    public async Task<(bool isValid, Guid userId)> VerifyAndConsumeAsync(string email, string otp, int maxAttempts, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var key = $"pwd:otp:{email}";
        var val = await db.StringGetAsync(key);
        if (val.IsNullOrEmpty) return (false, Guid.Empty);

        var payload = JsonSerializer.Deserialize<OtpPayload>(val!);
        if (payload is null) return (false, Guid.Empty);

        if (!FixedTimeEquals(payload.Otp, otp))
        {
            payload.Attempts++;
            if (payload.Attempts >= maxAttempts)
            {
                await db.KeyDeleteAsync(key);
            }
            else
            {
                var ttl = await db.KeyTimeToLiveAsync(key) ?? TimeSpan.FromMinutes(1);
                await db.StringSetAsync(key, JsonSerializer.Serialize(payload), ttl);
            }
            return (false, Guid.Empty);
        }

        await db.KeyDeleteAsync(key);
        return (true, payload.UserId);
    }

    private static string GenerateNumericOtp(int len)
    {
        Span<byte> buf = stackalloc byte[len];
        RandomNumberGenerator.Fill(buf);
        var sb = new StringBuilder(len);
        foreach (var b in buf) sb.Append((b % 10).ToString());
        return sb.ToString();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var res = 0;
        for (int i = 0; i < a.Length; i++) res |= a[i] ^ b[i];
        return res == 0;
    }

    private sealed class OtpPayload
    {
        public string Otp { get; set; } = default!;
        public Guid UserId { get; set; }
        public int Attempts { get; set; }
    }
}

public sealed class OtpOptions
{
    public int Length { get; set; } = 6;
    public int TtlMinutes { get; set; } = 10;
    public int MaxAttempts { get; set; } = 5;
    public int SendCooldownSeconds { get; set; } = 60;
    public int MaxSendsPerHour { get; set; } = 3;
}