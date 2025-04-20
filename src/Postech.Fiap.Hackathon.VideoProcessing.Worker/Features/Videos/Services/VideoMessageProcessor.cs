using Microsoft.Azure.Storage.Queue;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Services;

public class VideoMessageProcessor : IMessageProcessor
{
    public async Task ProcessVideoAsync(CloudQueueMessage message)
    {
        var videoPath = message.AsString;
        Console.WriteLine($"Recebido vídeo: {videoPath}");
        Console.WriteLine($"Processed video: {videoPath}");
    }
}