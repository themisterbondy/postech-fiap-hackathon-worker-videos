using Postech.Fiap.Hackathon.VideoProcessing.Worker;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;

var builder = WebApplication.CreateBuilder(args);
var configuration = AppSettings.Configuration();

builder.Services
    .AddWorker(configuration)
    .AddSerilogConfiguration(builder, configuration);

var app = builder.Build();

app.Run();