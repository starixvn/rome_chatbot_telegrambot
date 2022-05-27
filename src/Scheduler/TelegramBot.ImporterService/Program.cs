using Quartz;
using TelegramBot.ImporterService.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddFanexDataWithSqlServer(configuration)
    .AddFanexLogging(configuration)
    .AddFanexCachingMemory()
    .AddONEBookUM(configuration)
    .AddServices()
    .AddAlphaServicesClient(configuration)
    .AddQuartz(quartz =>
    {
        quartz.UseMicrosoftDependencyInjectionJobFactory();
        quartz.AddScheduleJobs(configuration, Fanex.Logging.Logger.Log);
    })
    .AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();
app.MapGet("/", () => "TelegramBot.Importer service!!!");
app.UseONEBookUM(configuration);

if (app.Environment.IsDevelopment())
{
    await app.RunAsync();
}
else
{
    await app.RunAsync("http://0.0.0.0:5000");
}
