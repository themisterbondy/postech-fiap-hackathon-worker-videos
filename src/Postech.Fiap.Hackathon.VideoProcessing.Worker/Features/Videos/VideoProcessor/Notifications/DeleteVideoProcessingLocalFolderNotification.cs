namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public class DeleteVideoProcessingLocalFolderNotificationHandler(
    ILogger<DeleteVideoProcessingLocalFolderNotificationHandler> logger) : INotificationHandler<
    DeleteVideoProcessingLocalFolderNotification>
{
    public Task Handle(DeleteVideoProcessingLocalFolderNotification notification, CancellationToken cancellationToken)
    {
        var localFolderPath = Path.Combine(Path.GetTempPath(), notification.VideoId);

        if (!Directory.Exists(localFolderPath)) return Task.CompletedTask;

        logger.LogInformation("Excluindo pasta local do processamento de vídeo: {VideoId}", notification.VideoId);
        Directory.Delete(localFolderPath, true);

        return Task.CompletedTask;
    }
}