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
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("O endereço de e-mail do destinatário não pode ser nulo ou vazio.", nameof(to));

        if (string.IsNullOrWhiteSpace(_settings.From))
            throw new ArgumentException("O endereço de e-mail do remetente não pode ser nulo ou vazio.",
                nameof(_settings.From));

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