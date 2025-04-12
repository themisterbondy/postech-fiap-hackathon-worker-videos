using Azure;
using Microsoft.Azure.Storage.Blob;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

public class StorageService(CloudBlobContainer container) : IStorageService
{
    public async Task<Result<string>> DownloadAsync(Guid videoId, string filePath, CancellationToken cancellationToken)
    {
        const string errorCode = "StorageService.DownloadAsync";

        if (string.IsNullOrWhiteSpace(filePath))
            return Result.Failure<string>(Error.Failure(errorCode, "O caminho do arquivo não pode estar vazio."));

        var blob = container.GetBlockBlobReference(filePath);
        if (!await blob.ExistsAsync(cancellationToken))
            return Result.Failure<string>(Error.Failure(errorCode, $"O blob '{filePath}' não existe no container."));

        try
        {
            var memoryStream = new MemoryStream();
            await blob.DownloadToStreamAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var folderPath = Path.Combine(Path.GetTempPath(), videoId.ToString());
            Directory.CreateDirectory(folderPath);

            var localPath = Path.Combine(folderPath, Path.GetFileName(filePath));
            await using var fileStream = File.Create(localPath);
            await memoryStream.CopyToAsync(fileStream, cancellationToken);

            return Result.Success(localPath);
        }
        catch (RequestFailedException ex)
        {
            return Result.Failure<string>(Error.Failure(errorCode, $"Erro ao baixar o blob: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure(errorCode, $"Erro inesperado: {ex.Message}"));
        }
    }

    public async Task<Result<string>> UploadAsync(Guid videoId, string filePath, string contentType,
        CancellationToken cancellationToken)
    {
        const string errorCode = "StorageService.UploadAsync";

        if (string.IsNullOrWhiteSpace(filePath))
            return Result.Failure<string>(Error.Failure(errorCode, "O caminho do arquivo não pode estar vazio."));

        try
        {
            var blobName = Path.Combine(videoId.ToString(), Path.GetFileName(filePath)).Replace("\\", "/");
            var blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = contentType;

            await using var fileStream = File.OpenRead(filePath);
            await blob.UploadFromStreamAsync(fileStream, cancellationToken);

            return Result.Success(blob.Uri.ToString());
        }
        catch (RequestFailedException ex)
        {
            return Result.Failure<string>(Error.Failure(errorCode, $"Erro ao fazer upload para o blob: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure(errorCode, $"Erro inesperado: {ex.Message}"));
        }
    }
}