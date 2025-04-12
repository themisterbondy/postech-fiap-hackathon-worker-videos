using System.Reflection;
using MediatR;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Common.Behavior;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Interfaces;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Jobs;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Repositores;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Features.Videos.VideoProcessor.Services;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Persistence;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;

namespace Postech.Fiap.Hackathon.VideoProcessing.Worker;

public static class DependencyInjection
{
    private static readonly Assembly Assembly = typeof(Program).Assembly;

    public static IServiceCollection AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatRConfiguration();
        var sqlServerConnectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(sqlServerConnectionString,
                options => { options.EnableRetryOnFailure(2, TimeSpan.FromSeconds(20), new List<int>()); });
            options.EnableSensitiveDataLogging(false);
        });

        services.AddSingleton<CloudBlobContainer>(sp =>
        {
            var connectionString = configuration["Azure:ConnectionString"];
            var containerName = configuration["Azure:Blob:Container"];

            var account = CloudStorageAccount.Parse(connectionString);
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            return container;
        });

        services.AddSingleton<CloudQueue>(_ =>
        {
            var connectionString = configuration["Azure:ConnectionString"];
            var queueName = configuration["Azure:Queue:Name"];

            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudQueueClient();

            var queue = client.GetQueueReference(queueName);
            queue.CreateIfNotExists();

            return queue;
        });

        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            var jobKey = new JobKey("VideoQueueJob");

            q.AddJob<VideoQueueJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("VideoQueueTrigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));
        });

        services.AddQuartzHostedService();

        services.AddScoped<IMessageReceiver, VideoMessageReceiver>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IVideoFrameExtractor, VideoFrameExtractor>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));


        return services;
    }

    private static void AddMediatRConfiguration(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
    }

    public static void AddSerilogConfiguration(this IServiceCollection services,
        WebApplicationBuilder builder, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var applicationName =
            $"{Assembly.GetName().Name?.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails()
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(Log.Logger, true);
    }
}