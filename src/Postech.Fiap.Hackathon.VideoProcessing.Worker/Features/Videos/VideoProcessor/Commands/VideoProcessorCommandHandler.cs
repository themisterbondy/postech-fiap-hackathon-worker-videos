using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;

public class VideoProcessorCommandHandler(
    IVideoRepository videoRepository,
    IStorageService storageService,
    IVideoFrameExtractor videoFrameExtractor,
    IMediator mediator,
    ILogger<VideoProcessorCommandHandler> logger)
    : IRequestHandler<VideoProcessorCommand, Result>
{
    public async Task<Result> Handle(VideoProcessorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var video = await videoRepository.GetVideoByIDAsync(request.VideoId, cancellationToken);

            if (video is null)
                return Result.Failure(Error.Failure("VideoProcessorCommandHandler.Handle",
                    $"Vídeo com o ID {request.VideoId} não encontrado."));

            if (video.Status != VideoStatus.Uploaded)
                return Result.Failure(Error.Failure("VideoProcessorCommandHandler.Handle",
                    $"O vídeo com o ID {video.Id} não está em um estado válido para processamento."));

            logger.LogInformation("Alterando o status do vídeo para 'Processando'. ID: {VideoId}", video.Id);
            await videoRepository.ChangeStatusAsync(video.Id.ToString(), VideoStatus.Processing, cancellationToken);
            logger.LogInformation("Status do vídeo alterado para 'Processando'. ID: {VideoId}", video.Id);

            logger.LogInformation("Iniciando o download do vídeo. ID: {VideoId}", video.Id);
            var videoDownloadResult = await storageService.DownloadAsync(
                video.Id,
                video.FilePath,
                cancellationToken);
            if (videoDownloadResult.IsFailure)
            {
                logger.LogError("Erro ao baixar o vídeo com ID: {VideoId}. Motivo: {Error}", video.Id,
                    videoDownloadResult.Error.Message);
                return Result.Failure(videoDownloadResult.Error);
            }

            logger.LogInformation("Vídeo baixado com sucesso. ID: {VideoId}", video.Id);

            logger.LogInformation("Iniciando a extração de frames do vídeo. ID: {VideoId}", video.Id);
            var framesResult = await videoFrameExtractor.ExtractFramesAsync(
                video.Id,
                videoDownloadResult.Value,
                video.ThumbnailsInterval);
            if (framesResult.IsFailure)
            {
                logger.LogError("Erro ao extrair frames do vídeo com ID: {VideoId}. Motivo: {Error}", video.Id,
                    framesResult.Error.Message);
                return Result.Failure(framesResult.Error);
            }

            logger.LogInformation("Frames do vídeo extraídos com sucesso. ID: {VideoId}", video.Id);

            logger.LogInformation("Iniciando o upload dos frames extraídos. ID: {VideoId}", video.Id);
            var uploadResult = await storageService.UploadAsync(
                video.Id,
                framesResult.Value,
                "application/x-zip-compressed",
                cancellationToken);
            if (uploadResult.IsFailure)
            {
                logger.LogError("Erro ao fazer upload dos frames do vídeo com ID: {VideoId}. Motivo: {Error}", video.Id,
                    uploadResult.Error.Message);
                return Result.Failure(uploadResult.Error);
            }

            logger.LogInformation("Upload dos frames realizado com sucesso. ID: {VideoId}", video.Id);

            logger.LogInformation("Atualizando o caminho do zip no banco de dados. ID: {VideoId}", video.Id);
            await videoRepository.UpdateZipPathAsync(video.Id.ToString(), uploadResult.Value, cancellationToken);
            logger.LogInformation("Caminho do zip atualizado com sucesso. ID: {VideoId}", video.Id);

            logger.LogInformation("Alterando o status do vídeo para 'Processado'. ID: {VideoId}", video.Id);
            await videoRepository.ChangeStatusAsync(video.Id.ToString(), VideoStatus.Processed, cancellationToken);
            logger.LogInformation("Status do vídeo alterado para 'Processado'. ID: {VideoId}", video.Id);

            // logger.LogInformation("Publicando notificação de sucesso do processamento do vídeo. ID: {VideoId}",
            //     video.Id);
            // await mediator.Publish(new VideoProcessingSucceededNotification(video.Id), cancellationToken);
            // logger.LogInformation(
            //     "Notificação de sucesso do processamento do vídeo publicada com sucesso. ID: {VideoId}", video.Id);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Erro inesperado ao processar o vídeo com ID: {VideoId}", request.VideoId);
            await videoRepository.ChangeStatusAsync(request.VideoId, VideoStatus.Uploaded, cancellationToken);
            await mediator.Publish(new DeleteVideoProcessingLocalFolderNotification(request.VideoId),
                cancellationToken);
            return Result.Failure(Error.Failure("VideoProcessorCommandHandler.Handle",
                $"Erro ao processar o vídeo com ID {request.VideoId}: {e.Message}"));
        }
    }
}