using System;

namespace Infrastructure.Common;

public class Options
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int AccessTokenMinutes { get; set; } = 10;
    }

    public sealed class RefreshOptions
    {
        public int ExpiryDays { get; set; } = 14;
    }

    public sealed class RedisOptions
    {
        public string? Configuration { get; set; }
        public int Database { get; set; } = 0;
    }

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string FromName { get; set; } = "My App";
    }

    public sealed class GoogleOAuthOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string AuthorizationUri { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
        public string TokenUri { get; set; } = "https://oauth2.googleapis.com/token";
        public string UserInfoUri { get; set; } = "https://openidconnect.googleapis.com/v1/userinfo";
        public string[] Scopes { get; set; } = new[] { "openid", "profile", "email" };
        public int StateTtlMinutes { get; set; } = 5;
        public int CodeVerifierLength { get; set; } = 64;
    }
}
