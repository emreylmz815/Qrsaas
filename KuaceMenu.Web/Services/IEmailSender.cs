using System;
using System.Threading.Tasks;
using KuaceMenu.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace KuaceMenu.Web.Services;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<EmailSettings> _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var settings = _settings.Value;
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(settings.Host, settings.Port, settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                await client.AuthenticateAsync(settings.Username, settings.Password);
            }
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderiminde hata oluştu");
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
