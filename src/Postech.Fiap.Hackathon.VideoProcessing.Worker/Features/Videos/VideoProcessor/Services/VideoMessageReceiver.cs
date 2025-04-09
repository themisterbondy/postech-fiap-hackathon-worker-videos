using Microsoft.Azure.Storage.Queue;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

public class VideoMessageReceiver(ISender sender) : IMessageReceiver
{
    public async Task ReceiverAsync(CloudQueueMessage message)
    {
        var videoId = message.AsString;
        Console.WriteLine($"Recebido vídeo: {videoId}");
        var downloadResult = await sender.Send(new VideoProcessorCommand(videoId));

        if (downloadResult.IsFailure)
        {
            Console.WriteLine($"Erro ao processar vídeo: {downloadResult.Error}");
            return;
        }

        Console.WriteLine($"Processed video: {videoId}");
    }
}