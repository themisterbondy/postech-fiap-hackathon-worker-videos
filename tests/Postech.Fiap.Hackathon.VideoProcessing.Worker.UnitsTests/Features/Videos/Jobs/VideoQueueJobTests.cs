using MediatR;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Jobs;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;
using Quartz;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Jobs;

public class VideoQueueJobTests
{
    private readonly IJobExecutionContext _jobContext = Substitute.For<IJobExecutionContext>();
    private readonly ILogger<VideoQueueJob> _logger = Substitute.For<ILogger<VideoQueueJob>>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CloudQueue _queue = Substitute.For<CloudQueue>(new Uri("https://fakequeue"), null);
    private readonly IMessageReceiver _receiver = Substitute.For<IMessageReceiver>();

    private VideoQueueJob CreateSut()
    {
        return new VideoQueueJob(_queue, _receiver, _mediator, _logger);
    }

    private static CloudQueueMessage CreateMessage(string id = "video123", int dequeueCount = 1)
    {
        var message = new CloudQueueMessage(id);
        typeof(CloudQueueMessage).GetProperty(nameof(CloudQueueMessage.DequeueCount))!
            .SetValue(message, dequeueCount);
        return message;
    }

    [Fact]
    public async Task Execute_ShouldProcessMessageAndPublishCleanup_WhenSuccess()
    {
        // Arrange
        var message = CreateMessage();
        _queue.GetMessageAsync(TimeSpan.FromMinutes(5), null, null, CancellationToken.None).Returns(message);
        _receiver.ReceiverAsync(message).Returns(Result.Success());

        var sut = CreateSut();

        // Act
        await sut.Execute(_jobContext);

        // Assert
        await _receiver.Received(1).ReceiverAsync(message);
        await _queue.Received(1).DeleteMessageAsync(message);
        await _mediator.Received(1).Publish(
            Arg.Is<DeleteVideoProcessingLocalFolderNotification>(n => n.VideoId == message.AsString),
            Arg.Any<CancellationToken>());
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Mensagem processada e removida da fila com sucesso")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Execute_ShouldRemoveMessageAndPublishError_WhenProcessingFails_At5Tries()
    {
        // Arrange
        var message = CreateMessage(dequeueCount: 5);
        _queue.GetMessageAsync(TimeSpan.FromMinutes(5), null, null, CancellationToken.None).Returns(message);
        _receiver.ReceiverAsync(message).Returns(Result.Failure(Error.Failure("Processor", "Falha final")));

        var sut = CreateSut();

        // Act
        await sut.Execute(_jobContext);

        // Assert
        await _queue.Received(1).DeleteMessageAsync(message);
        await _mediator.Received(1).Publish(
            Arg.Is<VideoProcessingFailedNotification>(n =>
                n.VideoId == message.AsString),
            Arg.Any<CancellationToken>());
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Mensagem falhou após 5 tentativas")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Execute_ShouldLogWarning_WhenProcessingFails_ButRetryCountLessThan5()
    {
        // Arrange
        var message = CreateMessage(dequeueCount: 3);
        _queue.GetMessageAsync(TimeSpan.FromMinutes(5), null, null, CancellationToken.None).Returns(message);
        _receiver.ReceiverAsync(message).Returns(Result.Failure(Error.Failure("Processor", "Falha parcial")));

        var sut = CreateSut();

        // Act
        await sut.Execute(_jobContext);

        // Assert
        await _queue.DidNotReceive().DeleteMessageAsync(message);
        await _mediator.DidNotReceive().Publish(Arg.Any<VideoProcessingFailedNotification>());
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Mensagem falhou. Tentativa 3 de 5")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Execute_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        _queue.GetMessageAsync(TimeSpan.FromMinutes(5), null, null, CancellationToken.None)
            .Returns(Task.FromException<CloudQueueMessage>(new Exception("queue falhou")));

        var sut = CreateSut();

        // Act
        await sut.Execute(_jobContext);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(msg => msg.ToString()!.Contains("Erro ao processar mensagem da fila")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Execute_ShouldDoNothing_WhenNoMessageIsReturned()
    {
        // Arrange
        _queue.GetMessageAsync(TimeSpan.FromMinutes(5), null, null, CancellationToken.None)
            .Returns((CloudQueueMessage?)null);

        var sut = CreateSut();

        // Act
        await sut.Execute(_jobContext);

        // Assert
        await _receiver.DidNotReceive().ReceiverAsync(Arg.Any<CloudQueueMessage>());
        await _queue.DidNotReceive().DeleteMessageAsync(Arg.Any<CloudQueueMessage>());
        await _mediator.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}