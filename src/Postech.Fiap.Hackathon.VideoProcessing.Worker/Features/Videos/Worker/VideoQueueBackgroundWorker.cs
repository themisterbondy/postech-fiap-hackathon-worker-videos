using Microsoft.Azure.Storage.Queue;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Interfaces;
using Quartz;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Worker;

public class VideoQueueJob(
    CloudQueue queue,
    IMessageProcessor processor,
    ILogger<VideoQueueJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var message = await queue.GetMessageAsync();

            if (message != null)
            {
                logger.LogInformation("Mensagem lida da fila.");

                await processor.ProcessVideoAsync(message);
                await queue.DeleteMessageAsync(message);

                logger.LogInformation("Mensagem processada e removida da fila.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar mensagem no VideoQueueJob.");
        }
    }
}