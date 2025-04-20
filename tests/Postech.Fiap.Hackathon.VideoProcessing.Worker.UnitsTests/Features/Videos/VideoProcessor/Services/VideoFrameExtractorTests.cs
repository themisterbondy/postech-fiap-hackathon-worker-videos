using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.VideoProcessor.Services;

public class VideoFrameExtractorTests
{
    private readonly ILogger<VideoFrameExtractor> _logger;
    private readonly VideoFrameExtractor _sut;

    public VideoFrameExtractorTests()
    {
        _logger = Substitute.For<ILogger<VideoFrameExtractor>>();
        _sut = new VideoFrameExtractor(_logger);
    }

    [Fact]
    public async Task ExtractFramesAsync_ShouldReturnSuccessAndCreateZip_WhenVideoIsValid()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var videoPath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "test_video.mp4");
        File.Exists(videoPath).Should().BeTrue("o vídeo de teste precisa estar presente");
        Console.WriteLine($"[DEBUG] Looking for video at: {videoPath}");
        Console.WriteLine($"[DEBUG] File exists: {File.Exists(videoPath)}");

        var expectedOutputDir = Path.Combine(Path.GetTempPath(), videoId.ToString(), $"frames_{videoId}");
        var expectedZipPath = $"{expectedOutputDir}.zip";

        // Opcional: limpar antes do teste
        if (Directory.Exists(expectedOutputDir)) Directory.Delete(expectedOutputDir, true);
        if (File.Exists(expectedZipPath)) File.Delete(expectedZipPath);

        // Act
        var result = await _sut.ExtractFramesAsync(videoId, videoPath, 50);

        // Assert
        result.IsSuccess.Should().BeTrue("o vídeo é válido e deve extrair frames");
        File.Exists(result.Value).Should().BeTrue("o arquivo zip com os frames deve ter sido criado");
        result.Value.Should().Be(expectedZipPath);

        // E os frames? Verifica se a pasta temporária tem imagens
        Directory.Exists(expectedOutputDir).Should().BeTrue();
        Directory.GetFiles(expectedOutputDir, "*.png").Length.Should()
            .BeGreaterThan(0, "pelo menos um frame deve ter sido extraído");

        // Verifica também se o .zip contém as imagens
        var tempExtractFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(expectedZipPath, tempExtractFolder);
        Directory.GetFiles(tempExtractFolder, "*.png").Should().NotBeEmpty("o zip deve conter os frames extraídos");
    }


    [Fact]
    public async Task ExtractFramesAsync_ShouldReturnFailure_WhenVideoPathIsInvalid()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var invalidPath = Path.Combine(AppContext.BaseDirectory, "TestAssets", "inexistent_video.mp4");

        // Act
        var result = await _sut.ExtractFramesAsync(videoId, invalidPath, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoFrameExtractor.ExtractFramesAsync");

        // Verifica se log de erro foi chamado corretamente
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Erro ao extrair frames")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>());
    }
}