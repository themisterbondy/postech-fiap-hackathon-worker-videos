using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.UnitsTests.Mocks;

public abstract class ApplicationDbContextMock
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        return context;
    }
}