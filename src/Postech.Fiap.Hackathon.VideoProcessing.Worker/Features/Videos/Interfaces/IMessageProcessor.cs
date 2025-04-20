using Microsoft.Azure.Storage.Queue;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Interfaces;

public interface IMessageProcessor
{
    Task ProcessVideoAsync(CloudQueueMessage message);
}