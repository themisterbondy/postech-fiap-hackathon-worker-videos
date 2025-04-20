using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IVideoFrameExtractor
{
    Task<Result<string>> ExtractFramesAsync(Guid videoId, string videoPath, int frameInterval);
}