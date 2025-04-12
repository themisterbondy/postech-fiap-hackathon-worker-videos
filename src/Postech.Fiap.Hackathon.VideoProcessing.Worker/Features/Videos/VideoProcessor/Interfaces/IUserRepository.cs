namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;

public interface IUserRepository
{
    public  Task<string> GetUserEmailByVideoId(string videoId);
}