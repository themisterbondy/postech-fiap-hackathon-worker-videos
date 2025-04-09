using Azure;
using Microsoft.Azure.Storage.Blob;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Contracts;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

public class VideoDownloader(CloudBlobContainer container) : IVideoDownloader
{
    public async Task<Result<DownloadResult>> DownloadAsync(string filePath)
    {
        const string errorCode = "VideoDownloader.DownloadAsync";

        if (string.IsNullOrWhiteSpace(filePath))
            return Result.Failure<DownloadResult>(Error.Failure(errorCode, "Path cannot be null or whitespace"));

        var blob = container.GetBlockBlobReference(filePath);
        if (!await blob.ExistsAsync())
            return Result.Failure<DownloadResult>(Error.Failure(errorCode, "Blob does not exist"));

        var memoryStream = new MemoryStream();
        try
        {
            await blob.DownloadToStreamAsync(memoryStream);
            memoryStream.Position = 0;

            var downloadResult = new DownloadResult(memoryStream,blob.Properties.ContentType);

            return Result.Success(downloadResult);
        }
        catch (RequestFailedException ex)
        {
            return Result.Failure<DownloadResult>(Error.Failure(errorCode, ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<DownloadResult>(Error.Failure(errorCode, ex.Message));
        }
    }
}