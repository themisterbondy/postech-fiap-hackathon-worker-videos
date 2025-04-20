using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<string> GetUserEmailByVideoId(string videoId)
    {
        var guid = Guid.Parse(videoId);

        var video = await context.Videos
            .Where(v => v.Id == guid)
            .FirstOrDefaultAsync(v => v.Id == guid); // força avaliação da primeira parte

        var users = await context.Users.FirstOrDefaultAsync(v =>
            v.Id == video.UserId.ToString()); // força avaliação da segunda parte

        return users.Email;
    }
}