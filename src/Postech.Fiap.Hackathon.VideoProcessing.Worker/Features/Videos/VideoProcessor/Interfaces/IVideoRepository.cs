using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IVideoRepository
{
    Task<Video?> GetVideoByIDAsync(string videoId, CancellationToken cancellationToken);
    Task ChangeStatusAsync(string videoId, VideoStatus status, CancellationToken cancellationToken);
    Task UpdateZipPathAsync(string videoId, string zipPath, CancellationToken cancellationToken);
}