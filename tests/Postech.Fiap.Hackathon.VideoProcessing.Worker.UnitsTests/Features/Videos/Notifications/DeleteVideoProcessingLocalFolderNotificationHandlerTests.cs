using Microsoft.Extensions.Logging;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Notifications;

public class DeleteVideoProcessingLocalFolderNotificationHandlerTests
{
    private readonly ILogger<DeleteVideoProcessingLocalFolderNotificationHandler> _logger;
    private readonly DeleteVideoProcessingLocalFolderNotificationHandler _sut;

    public DeleteVideoProcessingLocalFolderNotificationHandlerTests()
    {
        _logger = Substitute.For<ILogger<DeleteVideoProcessingLocalFolderNotificationHandler>>();
        _sut = new DeleteVideoProcessingLocalFolderNotificationHandler(_logger);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFolder_WhenFolderExists()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(Path.GetTempPath(), videoId);
        Directory.CreateDirectory(folderPath);
        await File.WriteAllTextAsync(Path.Combine(folderPath, "dummy.txt"), "arquivo de teste");

        var notification = new DeleteVideoProcessingLocalFolderNotification(videoId);

        // Sanity check
        Directory.Exists(folderPath).Should().BeTrue();

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        Directory.Exists(folderPath).Should().BeFalse("a pasta deve ser deletada");

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Excluindo pasta local")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldDoNothing_WhenFolderDoesNotExist()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(Path.GetTempPath(), videoId);

        if (Directory.Exists(folderPath))
            Directory.Delete(folderPath, true);

        var notification = new DeleteVideoProcessingLocalFolderNotification(videoId);

        // Act
        var act = () => _sut.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _logger.DidNotReceive().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}