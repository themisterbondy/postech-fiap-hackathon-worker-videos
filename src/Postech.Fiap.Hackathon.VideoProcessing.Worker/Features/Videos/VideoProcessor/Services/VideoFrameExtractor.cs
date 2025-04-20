using System.Drawing;
using System.IO.Compression;
using FFMpegCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

public class VideoFrameExtractor(ILogger<VideoFrameExtractor> logger) : IVideoFrameExtractor
{
    public async Task<Result<string>> ExtractFramesAsync(Guid videoId, string videoPath, int frameInterval)
    {
        const string errorCode = "VideoFrameExtractor.ExtractFramesAsync";

        try
        {
            var videoInfo = FFProbe.Analyse(videoPath);
            var duration = videoInfo.Duration;
            var interval = TimeSpan.FromSeconds(frameInterval);

            var outputFolder = Path.Combine(Path.GetTempPath(), videoId.ToString(), $"frames_{videoId}");
            Directory.CreateDirectory(outputFolder);

            for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
            {
                var outputPath = Path.Combine(outputFolder, $"frame_at_{(int)currentTime.TotalSeconds}.jpg");
                logger.LogInformation("Extraindo frame do vídeo em {CurrentTime}.", currentTime);

                await FFMpeg.SnapshotAsync(videoPath, outputPath, new Size(1920, 1080), currentTime);
            }

            var zipPath = $"{outputFolder}.zip";
            ZipFile.CreateFromDirectory(outputFolder, zipPath);

            logger.LogInformation("Extração finalizada. Arquivo compactado criado em: {ZipPath}", zipPath);
            return Result.Success(zipPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao extrair frames do vídeo com ID: {VideoId}", videoId);
            return Result.Failure<string>(Error.Failure(errorCode,
                $"Erro ao extrair os frames do vídeo: {ex.Message}"));
        }
    }
}