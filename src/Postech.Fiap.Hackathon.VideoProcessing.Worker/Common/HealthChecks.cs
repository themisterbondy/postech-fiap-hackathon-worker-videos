using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker.Common;

[ExcludeFromCodeCoverage]
public static class HealthChecks
{
    public static IServiceCollection AddUseHealthChecksConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IEndpointRouteBuilder UseHealthChecksConfiguration(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/status-text");
        app.MapHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = JsonSerializer.Serialize(
                        new
                        {
                            status = report.Status.ToString(),
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            monitors = report.Entries.Select(e => new
                                { key = e.Key, value = Enum.GetName(typeof(HealthStatus), e.Value.Status) })
                        });

                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            });


        return app;
    }
}