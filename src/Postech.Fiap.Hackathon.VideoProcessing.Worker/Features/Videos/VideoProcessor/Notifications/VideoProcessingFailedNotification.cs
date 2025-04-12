namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public class VideoProcessingFailedNotification(string videoId, string errorMessage) : INotification
{
    public string VideoId { get; set; } = videoId;
    public string ErrorMessage { get; set; } = errorMessage;
}