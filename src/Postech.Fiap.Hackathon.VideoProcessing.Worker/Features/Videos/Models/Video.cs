namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.Models;

public class Video
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int ThumbnailsInterval { get; set; }
    public string? ThumbnailsZipPath { get; set; } = string.Empty;
    public VideoStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}