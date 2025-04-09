using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;

public class VideoProcessorCommandHandler(
    ApplicationDbContext context,
    IVideoDownloader videoDownloader,
    ILogger<VideoProcessorCommandHandler> logger)
    : IRequestHandler<VideoProcessorCommand, Result<VideoProcessorResponse>>
{
    public async Task<Result<VideoProcessorResponse>> Handle(VideoProcessorCommand request,
        CancellationToken cancellationToken)
    {
        var video = await context.Videos.FirstOrDefaultAsync
        (
            v => v.Id == Guid.Parse(request.VideoId),
            cancellationToken: cancellationToken
        );

        if (video == null)
        {
            return Result.Failure<VideoProcessorResponse>(Error.Failure("VideoProcessorCommandHandler.Handle",
                $"Video with ID {request.VideoId} not found."));
        }

        // Baixar videos do blob
        var videoDownloadResult = await videoDownloader.DownloadAsync(video.FilePath);

        // Processar video estraindo frames
        // Mudar status do video para processando
        // Zipar os frames
        // Enviar os frames para o blob
        // Mudar status do video para processado


        return Result.Success(new VideoProcessorResponse());
    }
}