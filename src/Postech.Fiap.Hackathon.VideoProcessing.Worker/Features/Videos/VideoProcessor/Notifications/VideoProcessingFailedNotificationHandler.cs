using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public class VideoProcessingFailedNotificationHandler(
    IOptions<SmtpSettings> smtpSettings,
    IUserRepository userRepository,
    IVideoRepository videoRepository,
    ILogger<VideoProcessingFailedNotificationHandler> logger) : INotificationHandler<VideoProcessingFailedNotification>
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    public async Task Handle(VideoProcessingFailedNotification notification, CancellationToken cancellationToken)
    {
        await videoRepository.ChangeStatusAsync(notification.VideoId, VideoStatus.Failed, cancellationToken);

        logger.LogInformation("Enviando e-mail de erro para o usuário.");

        var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.From),
            Subject = $"[ALERTA] Erro no processamento do vídeo",
            Body = $"Ocorreu um erro ao processar o vídeo: {notification.ErrorMessage}",
            IsBodyHtml = false
        };

        var userEmail = await userRepository.GetUserEmailByVideoId(notification.VideoId);

        message.To.Add(userEmail);

        try
        {
            using var client = new SmtpClient();
            client.Host = _smtpSettings.Host;
            client.Port = _smtpSettings.Port;
            client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
            client.EnableSsl = _smtpSettings.EnableSsl;

            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("E-mail de erro enviado com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao enviar e-mail de notificação.");
        }
    }
}