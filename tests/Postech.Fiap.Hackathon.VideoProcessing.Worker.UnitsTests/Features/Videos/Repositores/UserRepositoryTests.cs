using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Authentication.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Features.Videos.Repositores;

public class UserRepositoryTests
{
    [Fact]
    public async Task Should_Create_User_Using_UserManager_And_FetchEmail()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var serviceProvider = services.BuildServiceProvider();

        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            UserName = "usuario@teste.com",
            Email = "usuario@teste.com"
        };

        var result = await userManager.CreateAsync(user, "SenhaForte123!");
        result.Succeeded.Should().BeTrue("o usuário deve ser criado com sucesso");

        var video = new Video
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(user.Id),
            FileName = "video.mp4",
            FilePath = "/tmp/video.mp4",
            CreatedAt = DateTime.UtcNow,
            Status = VideoStatus.Uploaded,
            ThumbnailsInterval = 5
        };

        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);

        // Act
        var email = await repo.GetUserEmailByVideoId(video.Id.ToString());

        // Assert
        email.Should().Be("usuario@teste.com");
    }
}