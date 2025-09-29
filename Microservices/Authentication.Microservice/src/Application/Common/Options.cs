namespace Infrastructure.Common;

public class Options
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Key { get; set; } = "";
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
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
        public string FromName { get; set; } = "My App";
    }

}