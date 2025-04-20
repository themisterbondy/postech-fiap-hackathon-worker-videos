using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Postech.Fiap.Orders.WebApi.Common;

[ExcludeFromCodeCoverage]
public static class HealthChecks
{
    public static IServiceCollection AddUseHealthChecksConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                name: "Azure SQL Server",
                connectionString: configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty,
                tags: ["Azure", "SQL", "DB"])
            .AddAzureBlobStorage(
                name: "Azure Blob Storage",
                connectionString: configuration["Azure:ConnectionString"] ?? string.Empty,
                tags: ["Azure", "Blob"])
            .AddAzureQueueStorage(
                name: "Azure Queue Storage",
                connectionString: configuration["Azure:ConnectionString"] ?? string.Empty,
                queueName: configuration["Azure:Queue:Name"] ?? string.Empty,
                tags: ["Azure", "Queue"]);

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