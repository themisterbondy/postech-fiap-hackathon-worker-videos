namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken);
}