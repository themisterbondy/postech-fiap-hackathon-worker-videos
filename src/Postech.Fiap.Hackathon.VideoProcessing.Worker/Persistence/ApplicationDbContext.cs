using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Video> Videos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}