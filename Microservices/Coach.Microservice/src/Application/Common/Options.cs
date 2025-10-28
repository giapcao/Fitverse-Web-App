namespace Infrastructure.Common;

public class Options
{
    public sealed class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string FromName { get; set; } = "Fitverse";
    }

    public sealed class CoachAppOptions
    {
        public string DashboardUrl { get; set; } = string.Empty;
    }
}

