using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("Videos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .IsRequired();

        builder.Property(v => v.UserId)
            .IsRequired();

        builder.Property(v => v.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.ThumbnailsInterval)
            .IsRequired();

        builder.Property(v => v.ThumbnailsZipPath)
            .HasMaxLength(500);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.CreatedAt)
            .IsRequired();
    }
}