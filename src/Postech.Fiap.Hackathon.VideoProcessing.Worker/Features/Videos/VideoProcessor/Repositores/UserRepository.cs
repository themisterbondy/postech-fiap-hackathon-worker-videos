using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<string> GetUserEmailByVideoId(string videoId)
    {
        var userEmail =await context.Database
            .SqlQueryRaw<string>("SELECT u.Email AS [Value] FROM Videos v JOIN AspNetUsers u ON v.UserId = u.Id WHERE v.Id = {0}", videoId)
            .FirstOrDefaultAsync();

        return userEmail;
    }
}