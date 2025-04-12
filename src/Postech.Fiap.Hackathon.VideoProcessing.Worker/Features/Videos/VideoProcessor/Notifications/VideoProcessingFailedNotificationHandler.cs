using Microsoft.Extensions.Options;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Infrastructure.Email;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public class VideoProcessingFailedNotificationHandler(
    IOptions<SmtpSettings> smtpSettings,
    IUserRepository userRepository,
    IVideoRepository videoRepository,
    ILogger<VideoProcessingFailedNotificationHandler> logger,
    IEmailSender emailSender)
    : INotificationHandler<VideoProcessingFailedNotification>
{
    public async Task Handle(VideoProcessingFailedNotification notification, CancellationToken cancellationToken)
    {
        await videoRepository.ChangeStatusAsync(notification.VideoId, VideoStatus.Failed, cancellationToken);

        logger.LogInformation("Enviando e-mail de erro para o usuário.");

        var userEmail = await userRepository.GetUserEmailByVideoId(notification.VideoId);

        try
        {
            await emailSender.SendAsync(
                userEmail,
                "[ALERTA] Erro no processamento do vídeo",
                $"Ocorreu um erro ao processar o vídeo: {notification.ErrorMessage}",
                cancellationToken);

            logger.LogInformation("E-mail de erro enviado com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao enviar e-mail de notificação.");
        }
    }
}