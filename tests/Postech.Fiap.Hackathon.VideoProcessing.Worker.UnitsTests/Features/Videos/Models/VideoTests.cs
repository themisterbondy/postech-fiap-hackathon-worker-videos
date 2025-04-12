using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Models;

public class VideoTests
{
    [Fact]
    public void Video_Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string fileName = "example.mp4";
        const string filePath = "/videos/example.mp4";
        const int thumbnailsInterval = 10;
        const VideoStatus status = VideoStatus.Uploaded;
        var createdAt = DateTime.UtcNow;

        // Act
        var video = new Video
        {
            Id = videoId,
            UserId = userId,
            FileName = fileName,
            FilePath = filePath,
            ThumbnailsInterval = thumbnailsInterval,
            Status = status,
            CreatedAt = createdAt
        };

        // Assert
        video.Id.Should().Be(videoId);
        video.UserId.Should().Be(userId);
        video.FileName.Should().Be(fileName);
        video.FilePath.Should().Be(filePath);
        video.ThumbnailsInterval.Should().Be(thumbnailsInterval);
        video.Status.Should().Be(status);
        video.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void Video_ShouldInitializeWithDefaultFileName()
    {
        // Act
        var video = new Video();

        // Assert
        video.FileName.Should().BeEmpty();
    }

    [Fact]
    public void Video_ShouldInitializeWithDefaultThumbnailsZipPath()
    {
        // Act
        var video = new Video();

        // Assert
        video.ThumbnailsZipPath.Should().BeEmpty();
    }

    [Fact]
    public void Video_ShouldAllowChangingStatus()
    {
        // Arrange
        var video = new Video { Status = VideoStatus.Uploaded };

        // Act
        video.Status = VideoStatus.Processed;

        // Assert
        video.Status.Should().Be(VideoStatus.Processed);
    }

    [Fact]
    public void Video_ShouldInitializeWithCreatedAt()
    {
        // Act
        var createdAt = DateTime.UtcNow;
        var video = new Video { CreatedAt = createdAt };

        // Assert
        video.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromMilliseconds(1));
    }
}