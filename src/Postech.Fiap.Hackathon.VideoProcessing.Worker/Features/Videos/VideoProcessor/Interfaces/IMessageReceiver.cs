using Microsoft.Azure.Storage.Queue;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IMessageReceiver
{
    Task ReceiverAsync(CloudQueueMessage message);
}