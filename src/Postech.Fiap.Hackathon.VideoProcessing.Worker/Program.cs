using Postech.Fiap.Hackathon.VideoProcessing.Worker;
using Postech.Fiap.Hackathon.VideoProcessing.Worker.Settings;
using Postech.Fiap.Orders.WebApi.Common;

var builder = WebApplication.CreateBuilder(args);
var configuration = AppSettings.Configuration();

builder.Services
    .AddWorker(configuration)
    .AddSerilogConfiguration(builder, configuration);

var app = builder.Build();

app.UseHealthChecksConfiguration();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program;