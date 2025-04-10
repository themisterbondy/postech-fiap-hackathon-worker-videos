using Microsoft.Azure.Storage.Queue;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

public class VideoMessageReceiver(ISender sender, ILogger<VideoMessageReceiver> logger) : IMessageReceiver
{
    public async Task<Result> ReceiverAsync(CloudQueueMessage message)
    {
        var videoId = message.AsString;
        logger.LogInformation("Recebido vídeo: {VideoId}", videoId);

        var processingResult = await sender.Send(new VideoProcessorCommand(videoId));
        if (processingResult.IsFailure)
        {
            logger.LogError("Erro ao processar vídeo: {Error}", processingResult.Error);
            return Result.Failure(processingResult.Error);
        }

        logger.LogInformation("Vídeo processado com sucesso: {VideoId}", videoId);
        return Result.Success();
    }
}