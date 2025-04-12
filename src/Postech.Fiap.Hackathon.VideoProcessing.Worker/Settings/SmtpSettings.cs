namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;

[ExcludeFromCodeCoverage]
public class SmtpSettings
{
    public string? Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public bool EnableSsl { get; set; }

    public string From { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
}