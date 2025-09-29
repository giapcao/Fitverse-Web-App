using Application.Abstractions.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Options = Infrastructure.Common.Options;

namespace Infrastructure.Gmail;

public class GmailSmtpEmailSender : IEmailSender
{
    private readonly Options.SmtpOptions _opt;
    public GmailSmtpEmailSender(IOptions<Options.SmtpOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_opt.User, _opt.Pass);

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName, _opt.User));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}
