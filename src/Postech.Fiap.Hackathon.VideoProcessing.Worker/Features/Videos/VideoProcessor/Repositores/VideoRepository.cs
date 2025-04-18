using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;

public class VideoRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IVideoRepository
{
    public async Task<Video?> GetVideoByIDAsync(string videoId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var video = await context.Videos.FirstOrDefaultAsync
        (
            v => v.Id == Guid.Parse(videoId),
            cancellationToken
        );

        return video;
    }

    public async Task ChangeStatusAsync(string videoId, VideoStatus status, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var video = await context.Videos.FirstOrDefaultAsync(v => v.Id == Guid.Parse(videoId),
            cancellationToken);

        if (video == null) return;

        video.Status = status;
        context.Videos.Update(video);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateZipPathAsync(string videoId, string zipPath, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var video = await context.Videos.FirstOrDefaultAsync(v => v.Id == Guid.Parse(videoId),
            cancellationToken);

        if (video == null) return;

        video.ThumbnailsZipPath = zipPath;
        context.Videos.Update(video);
        await context.SaveChangesAsync(cancellationToken);
    }
}