using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IStorageService
{
    Task<Result<string>> DownloadAsync(Guid videoId, string filePath, CancellationToken cancellationToken);

    Task<Result<string>> UploadAsync(Guid videoId, string filePath, string contentType,
        CancellationToken cancellationToken);
}