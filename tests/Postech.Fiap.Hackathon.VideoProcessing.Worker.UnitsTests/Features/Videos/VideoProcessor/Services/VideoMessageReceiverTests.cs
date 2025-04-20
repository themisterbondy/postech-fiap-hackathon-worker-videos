using MediatR;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.VideoProcessor.Services;

public class VideoMessageReceiverTests
{
    private readonly ILogger<VideoMessageReceiver> _logger;
    private readonly IMediator _mediator;
    private readonly VideoMessageReceiver _sut;

    public VideoMessageReceiverTests()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<VideoMessageReceiver>>();
        _sut = new VideoMessageReceiver(_mediator, _logger);
    }

    [Fact]
    public async Task ReceiverAsync_ShouldReturnSuccess_AndLog_WhenVideoIsProcessedSuccessfully()
    {
        // Arrange
        var message = new CloudQueueMessage("video-123");
        _mediator.Send(Arg.Any<VideoProcessorCommand>()).Returns(Result.Success());

        // Act
        var result = await _sut.ReceiverAsync(message);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verifica log de início
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Recebido vídeo: video-123")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        // Verifica log de sucesso
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Vídeo processado com sucesso")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        // Verifica se o comando foi enviado corretamente
        await _mediator.Received(1).Send(
            Arg.Is<VideoProcessorCommand>(c => c.VideoId == "video-123"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReceiverAsync_ShouldReturnFailure_AndLogError_WhenProcessingFails()
    {
        // Arrange
        var message = new CloudQueueMessage("video-456");
        var error = Error.Failure("Processor.Fail", "Falha no processamento");

        _mediator.Send(Arg.Any<VideoProcessorCommand>()).Returns(Result.Failure(error));

        // Act
        var result = await _sut.ReceiverAsync(message);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        // Verifica log de erro
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Erro ao processar vídeo")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}