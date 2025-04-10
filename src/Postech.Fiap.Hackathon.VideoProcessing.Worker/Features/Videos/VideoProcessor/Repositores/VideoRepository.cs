using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;

public class VideoRepository(ApplicationDbContext context) : IVideoRepository
{
    public async Task<Video?> GetVideoByIDAsync(string videoId, CancellationToken cancellationToken)
    {
        var video = await context.Videos.FirstOrDefaultAsync
        (
            v => v.Id == Guid.Parse(videoId),
            cancellationToken: cancellationToken
        );

        return video;
    }

    public Task ChangeStatusAsync(string videoId, VideoStatus status, CancellationToken cancellationToken)
    {
        var video = context.Videos.FirstOrDefault(v => v.Id == Guid.Parse(videoId));

        if (video == null) return Task.CompletedTask;

        video.Status = status;
        context.Videos.Update(video);
        return context.SaveChangesAsync(cancellationToken);

    }
}