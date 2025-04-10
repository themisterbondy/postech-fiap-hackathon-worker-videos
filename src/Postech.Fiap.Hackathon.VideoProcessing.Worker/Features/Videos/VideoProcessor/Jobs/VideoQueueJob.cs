using Microsoft.Azure.Storage.Queue;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Quartz;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Jobs;

public class VideoQueueJob(
    CloudQueue queue,
    IMessageReceiver receiver,
    ILogger<VideoQueueJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var message = await queue.GetMessageAsync(context.CancellationToken);

            if (message != null)
            {
                logger.LogInformation("Mensagem lida da fila. ID: {MessageId}, Conteúdo: {Message}", message.Id,
                    message.AsString);

                var messageResult = await receiver.ReceiverAsync(message);
                if (messageResult.IsFailure)
                {
                    if (message.DequeueCount >= 5)
                    {
                        await queue.DeleteMessageAsync(message);
                        logger.LogError("Mensagem falhou após 5 tentativas. Removendo da fila. ID: {MessageId}", message.Id);
                        return;
                    }

                    logger.LogWarning("Mensagem falhou. Tentativa {AttemptCount} de 5. ID: {MessageId}", message.DequeueCount, message.Id);
                    return;
                }

                await queue.DeleteMessageAsync(message);

                logger.LogInformation("Mensagem processada e removida da fila com sucesso. ID: {MessageId}",
                    message.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar mensagem da fila: {ExMessage}", ex.Message);
        }
    }
}