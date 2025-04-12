using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Notifications;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Infrastructure.Email;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Notifications;

public class VideoProcessingFailedNotificationHandlerTests
{
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();

    private readonly ILogger<VideoProcessingFailedNotificationHandler> _logger =
        Substitute.For<ILogger<VideoProcessingFailedNotificationHandler>>();

    private readonly SmtpSettings _smtpSettings = new()
    {
        Host = "smtp.test.com",
        Port = 587,
        Username = "user@test.com",
        Password = "password",
        From = "noreply@test.com",
        EnableSsl = false
    };

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IVideoRepository _videoRepository = Substitute.For<IVideoRepository>();

    private VideoProcessingFailedNotificationHandler CreateSut()
    {
        return new VideoProcessingFailedNotificationHandler(
            Options.Create(_smtpSettings),
            _userRepository,
            _videoRepository,
            _logger,
            _emailSender
        );
    }

    [Fact]
    public async Task Handle_ShouldSendEmailAndUpdateStatus_WhenNotificationIsValid()
    {
        // Arrange
        var notification = new VideoProcessingFailedNotification("video123", "Erro simulado");
        var userEmail = "user@example.com";
        _userRepository.GetUserEmailByVideoId(notification.VideoId).Returns(userEmail);

        var sut = CreateSut();

        // Act
        await sut.Handle(notification, CancellationToken.None);

        // Assert
        await _videoRepository.Received(1)
            .ChangeStatusAsync(notification.VideoId, VideoStatus.Failed, Arg.Any<CancellationToken>());
        await _userRepository.Received(1).GetUserEmailByVideoId(notification.VideoId);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Enviando e-mail de erro")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenEmailSenderThrows()
    {
        // Arrange
        var notification = new VideoProcessingFailedNotification("video123", "Falha no processamento");
        const string userEmail = "user@example.com";
        _userRepository.GetUserEmailByVideoId(notification.VideoId).Returns(userEmail);

        _emailSender
            .When(x => x.SendAsync(userEmail, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("SMTP quebrado"));

        var sut = CreateSut();

        // Act
        await sut.Handle(notification, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Erro ao enviar e-mail")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}