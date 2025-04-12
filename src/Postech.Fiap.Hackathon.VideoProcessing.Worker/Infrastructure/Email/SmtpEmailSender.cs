using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Infrastructure.Email;

[ExcludeFromCodeCoverage]
public class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(to);

        using var client = new SmtpClient
        {
            Host = _settings.Host,
            Port = _settings.Port,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSsl
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}