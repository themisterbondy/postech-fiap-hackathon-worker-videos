using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.ResultPattern;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Commands;

public record VideoProcessorCommand(string VideoId) : IRequest<Result>;