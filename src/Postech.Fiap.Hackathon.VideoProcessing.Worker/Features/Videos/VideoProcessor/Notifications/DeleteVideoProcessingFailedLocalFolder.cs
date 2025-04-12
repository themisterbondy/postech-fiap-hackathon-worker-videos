namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public class DeleteVideoProcessingLocalFolderNotification(string videoId) : INotification
{
    public string VideoId { get; set; } = videoId;
}