using Microsoft.Azure.Storage.Blob;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.VideoProcessor.Services;

public class StorageServiceTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly CloudBlobContainer _containerMock;
    private readonly StorageService _sut;

    public StorageServiceTests()
    {
        _containerMock = Substitute.For<CloudBlobContainer>(new Uri("https://test.blob.core.windows.net/test"));
        _sut = new StorageService(_containerMock);
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnFailure_WhenFilePathIsEmpty()
    {
        // Act
        var result = await _sut.DownloadAsync(Guid.NewGuid(), "", _cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não pode estar vazio");
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnFailure_WhenBlobDoesNotExist()
    {
        // Arrange
        var blobMock = Substitute.For<CloudBlockBlob>(new Uri("https://test.blob.core.windows.net/test/blob.mp4"));
        _containerMock.GetBlockBlobReference("video.mp4").Returns(blobMock);
        blobMock.ExistsAsync(_cancellationToken).Returns(false);

        // Act
        var result = await _sut.DownloadAsync(Guid.NewGuid(), "video.mp4", _cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não existe no container");
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnSuccess_WhenBlobIsDownloaded()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var filePath = "test.mp4";
        var blobMock = Substitute.For<CloudBlockBlob>(new Uri("https://test.blob.core.windows.net/test/test.mp4"));

        _containerMock.GetBlockBlobReference(filePath).Returns(blobMock);
        blobMock.ExistsAsync(_cancellationToken).Returns(true);
        blobMock
            .When(x => x.DownloadToStreamAsync(Arg.Any<Stream>(), _cancellationToken))
            .Do(info =>
            {
                var stream = info.Arg<Stream>();
                using var writer = new StreamWriter(stream, leaveOpen: true);
                writer.Write("mock content");
                writer.Flush();
                stream.Position = 0;
            });

        var tempFolder = Path.Combine(Path.GetTempPath(), videoId.ToString());
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);

        // Act
        var result = await _sut.DownloadAsync(videoId, filePath, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnFailure_WhenFilePathIsEmpty()
    {
        // Act
        var result = await _sut.UploadAsync(Guid.NewGuid(), "", "video/mp4", _cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não pode estar vazio");
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnSuccess_WhenUploadSucceeds()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "dummy data");

        var blobMock = Substitute.For<CloudBlockBlob>(new Uri("https://test.blob.core.windows.net/test/video.mp4"));
        _containerMock
            .GetBlockBlobReference(Arg.Any<string>())
            .Returns(blobMock);

        blobMock
            .UploadFromStreamAsync(Arg.Any<Stream>(), _cancellationToken)
            .Returns(Task.CompletedTask);

        var videoId = Guid.NewGuid();
        var fileName = Path.GetFileName(filePath);

        // Act
        var result = await _sut.UploadAsync(videoId, filePath, "video/mp4", _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain($"{videoId}/{fileName}");
    }

    [Fact]
    public async Task UploadAsync_ShouldReturnFailure_WhenUploadThrowsException()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "dummy data", _cancellationToken);

        var blobMock = Substitute.For<CloudBlockBlob>(new Uri("https://test.blob.core.windows.net/test/video.mp4"));
        _containerMock
            .GetBlockBlobReference(Arg.Any<string>())
            .Returns(blobMock);

        blobMock
            .When(x => x.UploadFromStreamAsync(Arg.Any<Stream>(), _cancellationToken))
            .Do(x => throw new Exception("upload failed"));

        // Act
        var result = await _sut.UploadAsync(Guid.NewGuid(), filePath, "video/mp4", _cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("upload failed");
    }
}