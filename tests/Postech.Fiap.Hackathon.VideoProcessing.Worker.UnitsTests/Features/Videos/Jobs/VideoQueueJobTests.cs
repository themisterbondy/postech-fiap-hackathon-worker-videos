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
        _queue.GetMessageAsync(Arg.Any<CancellationToken>()).Returns(message);
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
    }

    [Fact]
    public async Task Execute_ShouldRemoveMessageAndPublishError_WhenProcessingFails_At5Tries()
    {
        // Arrange
        var message = CreateMessage(dequeueCount: 5);
        _queue.GetMessageAsync(Arg.Any<CancellationToken>()).Returns(message);
        _receiver.ReceiverAsync(message)
            .Returns(Result.Failure(Error.Failure("Processor", "Falha final")));

        var sut = CreateSut();

        // Act
        await sut.Execute(_jobContext);

        // Assert
        await _queue.Received(1).DeleteMessageAsync(message);
        await _mediator.Received(1).Publish(
            Arg.Is<VideoProcessingFailedNotification>(n => n.VideoId == message.AsString),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        _queue.GetMessageAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CloudQueueMessage>(new Exception("queue falhou")));

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
}