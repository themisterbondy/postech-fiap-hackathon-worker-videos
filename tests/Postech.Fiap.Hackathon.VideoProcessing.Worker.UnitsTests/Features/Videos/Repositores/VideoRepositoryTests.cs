using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Mocks;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Repositores;

public class VideoRepositoryTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private static VideoRepository CreateRepository(out ApplicationDbContext context)
    {
        context = ApplicationDbContextMock.Create();
        var factory = Substitute.For<IDbContextFactory<ApplicationDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>()).Returns(context);
        return new VideoRepository(factory);
    }

    [Fact]
    public async Task GetVideoByIDAsync_ShouldReturnVideo_WhenExists()
    {
        // Arrange
        var repo = CreateRepository(out var context);
        var videoId = Guid.NewGuid();

        var video = new Video
        {
            Id = videoId,
            UserId = Guid.NewGuid(),
            FileName = "video.mp4",
            FilePath = "/tmp/video.mp4",
            Status = VideoStatus.Uploaded,
            CreatedAt = DateTime.UtcNow,
            ThumbnailsInterval = 5
        };

        await context.Videos.AddAsync(video, _cancellationToken);
        await context.SaveChangesAsync(_cancellationToken);

        // Act
        var result = await repo.GetVideoByIDAsync(videoId.ToString(), _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(videoId);
        result.FileName.Should().Be("video.mp4");
    }

    [Fact]
    public async Task GetVideoByIDAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var repo = CreateRepository(out _);

        // Act
        var result = await repo.GetVideoByIDAsync(Guid.NewGuid().ToString(), _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_ShouldDoNothing_WhenVideoDoesNotExist()
    {
        // Arrange
        var repo = CreateRepository(out _);

        // Act & Assert
        var act = async () =>
            await repo.ChangeStatusAsync(Guid.NewGuid().ToString(), VideoStatus.Failed, _cancellationToken);

        await act.Should().NotThrowAsync();
    }
}