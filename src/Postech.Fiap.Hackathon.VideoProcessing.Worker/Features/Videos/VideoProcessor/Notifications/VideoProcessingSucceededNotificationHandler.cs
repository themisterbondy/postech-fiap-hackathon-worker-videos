using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Infrastructure.Email;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public class VideoProcessingSucceededNotificationHandler(
    IUserRepository userRepository,
    ILogger<VideoProcessingSucceededNotificationHandler> logger,
    IEmailSender emailSender)
    : INotificationHandler<VideoProcessingSucceededNotification>
{
    public async Task Handle(VideoProcessingSucceededNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Enviando e-mail de sucesso para o usuário.");

        var videoId = notification.VideoId.ToString();

        var userEmail = await userRepository.GetUserEmailByVideoId(videoId);

        try
        {
            await emailSender.SendAsync(
                userEmail,
                "[SUCESSO] Vídeo processado com êxito",
                """
                O vídeo foi processado com sucesso! ID do vídeo: {videoId}. 
                As imagens geradas estão disponíveis para download.
                """,
                cancellationToken);

            logger.LogInformation("E-mail de sucesso enviado com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao enviar e-mail de notificação de sucesso.");
        }
    }
}