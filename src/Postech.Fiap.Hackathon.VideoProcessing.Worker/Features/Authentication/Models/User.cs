using Microsoft.AspNetCore.Identity;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Authentication.Models;

[ExcludeFromCodeCoverage]
public class User : IdentityUser
{
    public string? Initials { get; set; }
}