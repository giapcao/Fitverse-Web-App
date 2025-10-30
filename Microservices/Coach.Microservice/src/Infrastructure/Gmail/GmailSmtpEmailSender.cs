using System;
using Application.Abstractions.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Options = Infrastructure.Common.Options;

namespace Infrastructure.Gmail;

public class GmailSmtpEmailSender : IEmailSender
{
    private readonly Options.SmtpOptions _opt;
    private readonly ILogger<GmailSmtpEmailSender> _logger;

    public GmailSmtpEmailSender(IOptions<Options.SmtpOptions> opt, ILogger<GmailSmtpEmailSender> logger)
    {
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        using var client = new SmtpClient();
        try
        {
            _logger.LogDebug("Connecting to SMTP host {Host}:{Port} using StartTLS.", _opt.Host, _opt.Port);
            await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_opt.User, _opt.Pass, ct);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_opt.FromName, _opt.User));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            _logger.LogDebug("Sending email to {Recipient}.", to);
            await client.SendAsync(message, ct);
            _logger.LogDebug("Email sent to {Recipient}.", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}.", to);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, CancellationToken.None);
            }
        }
    }
}
