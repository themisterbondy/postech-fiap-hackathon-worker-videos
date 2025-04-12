using Microsoft.Extensions.Logging;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

namespace Postech.Fiap.Hackathon.VideoProcessing.Tests.Features.Videos.VideoProcessor.Commands;

public class VideoProcessorCommandHandlerTests
{
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly IVideoFrameExtractor _frameExtractor = Substitute.For<IVideoFrameExtractor>();

    private readonly ILogger<VideoProcessorCommandHandler> _logger =
        Substitute.For<ILogger<VideoProcessorCommandHandler>>();

    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IVideoRepository _videoRepo = Substitute.For<IVideoRepository>();

    private VideoProcessorCommandHandler CreateSut()
    {
        return new VideoProcessorCommandHandler(
            _videoRepo,
            _storageService,
            _frameExtractor,
            _logger);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenVideoDoesNotExist()
    {
        // Arrange
        _videoRepo.GetVideoByIDAsync("invalid-id", _ct).Returns((Video?)null);

        var sut = CreateSut();
        var command = new VideoProcessorCommand("invalid-id");

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenVideoStatusIsNotUploaded()
    {
        // Arrange
        var video = new Video { Id = Guid.NewGuid(), Status = VideoStatus.Processed };
        _videoRepo.GetVideoByIDAsync(video.Id.ToString(), _ct).Returns(video);

        var sut = CreateSut();
        var command = new VideoProcessorCommand(video.Id.ToString());

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não está em um estado válido");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenDownloadFails()
    {
        // Arrange
        var video = new Video
        {
            Id = Guid.NewGuid(),
            Status = VideoStatus.Uploaded,
            FilePath = "video.mp4"
        };

        _videoRepo.GetVideoByIDAsync(video.Id.ToString(), _ct).Returns(video);
        _storageService.DownloadAsync(video.Id, video.FilePath, _ct)
            .Returns(Result.Failure<string>(Error.Failure("Download", "Falha no download")));

        var sut = CreateSut();
        var command = new VideoProcessorCommand(video.Id.ToString());

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Falha no download");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenExtractFramesFails()
    {
        // Arrange
        var video = new Video
        {
            Id = Guid.NewGuid(),
            Status = VideoStatus.Uploaded,
            FilePath = "video.mp4",
            ThumbnailsInterval = 5
        };

        _videoRepo.GetVideoByIDAsync(video.Id.ToString(), _ct).Returns(video);
        _storageService.DownloadAsync(video.Id, video.FilePath, _ct).Returns(Result.Success("/tmp/video.mp4"));
        _frameExtractor.ExtractFramesAsync(video.Id, "/tmp/video.mp4", 5)
            .Returns(Result.Failure<string>(Error.Failure("Frames", "Erro na extração")));

        var sut = CreateSut();
        var command = new VideoProcessorCommand(video.Id.ToString());

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Erro na extração");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenUploadFails()
    {
        // Arrange
        var video = new Video
        {
            Id = Guid.NewGuid(),
            Status = VideoStatus.Uploaded,
            FilePath = "video.mp4",
            ThumbnailsInterval = 5
        };

        _videoRepo.GetVideoByIDAsync(video.Id.ToString(), _ct).Returns(video);
        _storageService.DownloadAsync(video.Id, video.FilePath, _ct).Returns(Result.Success("/tmp/video.mp4"));
        _frameExtractor.ExtractFramesAsync(video.Id, "/tmp/video.mp4", 5)
            .Returns(Result.Success("/tmp/frames.zip"));
        _storageService.UploadAsync(video.Id, "/tmp/frames.zip", "application/x-zip-compressed", _ct)
            .Returns(Result.Failure<string>(Error.Failure("Upload", "Falha no upload")));

        var sut = CreateSut();
        var command = new VideoProcessorCommand(video.Id.ToString());

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Falha no upload");
    }

    [Fact]
    public async Task Should_ProcessVideoSuccessfully()
    {
        // Arrange
        var video = new Video
        {
            Id = Guid.NewGuid(),
            Status = VideoStatus.Uploaded,
            FilePath = "video.mp4",
            ThumbnailsInterval = 5
        };

        _videoRepo.GetVideoByIDAsync(video.Id.ToString(), _ct).Returns(video);
        _storageService.DownloadAsync(video.Id, video.FilePath, _ct).Returns(Result.Success("/tmp/video.mp4"));
        _frameExtractor.ExtractFramesAsync(video.Id, "/tmp/video.mp4", 5)
            .Returns(Result.Success("/tmp/frames.zip"));
        _storageService.UploadAsync(video.Id, "/tmp/frames.zip", "application/x-zip-compressed", _ct)
            .Returns(Result.Success("https://blob"));

        var sut = CreateSut();
        var command = new VideoProcessorCommand(video.Id.ToString());

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _videoRepo.Received().ChangeStatusAsync(video.Id.ToString(), VideoStatus.Processing, _ct);
        await _videoRepo.Received().ChangeStatusAsync(video.Id.ToString(), VideoStatus.Processed, _ct);
    }

    [Fact]
    public async Task Should_HandleUnexpectedException_AndSetStatusToFailed()
    {
        // Arrange
        var video = new Video { Id = Guid.NewGuid(), Status = VideoStatus.Uploaded };

        _videoRepo.GetVideoByIDAsync(video.Id.ToString(), _ct).Returns(video);
        _storageService.DownloadAsync(Arg.Any<Guid>(), Arg.Any<string>(), _ct)
            .Returns<Task<Result<string>>>(_ => throw new Exception("erro inesperado"));

        var sut = CreateSut();
        var command = new VideoProcessorCommand(video.Id.ToString());

        // Act
        var result = await sut.Handle(command, _ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("erro inesperado");

        await _videoRepo.Received().ChangeStatusAsync(video.Id.ToString(), VideoStatus.Failed, _ct);
    }
}