namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

public record VideoProcessingSucceededNotification(Guid VideoId) : INotification;