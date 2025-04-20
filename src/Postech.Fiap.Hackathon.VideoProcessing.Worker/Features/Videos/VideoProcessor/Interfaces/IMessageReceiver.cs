using Microsoft.Azure.Storage.Queue;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IMessageReceiver
{
    Task<Result> ReceiverAsync(CloudQueueMessage message);
}