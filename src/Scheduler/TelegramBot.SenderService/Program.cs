using Quartz;
using TelegramBot.SenderService.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddFanexDataWithSqlServer(configuration)
    .AddFanexCachingMemory()
    .AddFanexLogging(configuration)
    .AddONEBookUM(configuration)
    .AddServices(configuration)
    .AddQuartz(quartz =>
    {
        quartz.UseMicrosoftDependencyInjectionJobFactory();
        quartz.AddNotificationSenderJob(configuration, Fanex.Logging.Logger.Log);
    })
    .AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();
app.MapGet("/", () => "TelegramBot.Sender service!!!");
app.UseONEBookUM(configuration);

if (app.Environment.IsDevelopment())
{
    await app.RunAsync();
}
else
{
    await app.RunAsync("http://0.0.0.0:5000");
}
