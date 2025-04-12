using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Mocks;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Repositores;

public class VideoRepositoryTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task GetVideoByIDAsync_ShouldReturnVideo_WhenExists()
    {
        // Arrange
        var context = ApplicationDbContextMock.Create();
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

        var repo = new VideoRepository(context);

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
        var context = ApplicationDbContextMock.Create();
        var repo = new VideoRepository(context);

        // Act
        var result = await repo.GetVideoByIDAsync(Guid.NewGuid().ToString(), _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_ShouldChangeStatus_WhenVideoExists()
    {
        // Arrange
        var context = ApplicationDbContextMock.Create();
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

        var repo = new VideoRepository(context);

        // Act
        await repo.ChangeStatusAsync(videoId.ToString(), VideoStatus.Processed, _cancellationToken);

        // Assert
        var updatedVideo = await context.Videos.FindAsync(videoId);
        updatedVideo!.Status.Should().Be(VideoStatus.Processed);
    }

    [Fact]
    public async Task ChangeStatusAsync_ShouldDoNothing_WhenVideoDoesNotExist()
    {
        // Arrange
        var context = ApplicationDbContextMock.Create();
        var repo = new VideoRepository(context);

        // Act
        var action = async () =>
            await repo.ChangeStatusAsync(Guid.NewGuid().ToString(), VideoStatus.Failed, _cancellationToken);

        // Assert
        await action.Should().NotThrowAsync();
    }
}