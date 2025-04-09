using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Contracts;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IVideoDownloader
{
    Task<Result<DownloadResult>> DownloadAsync(string filePath);
}