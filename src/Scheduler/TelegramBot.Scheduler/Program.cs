using Quartz;
using TelegramBot.Scheduler.Configuration;
using TelegramBot.Scheduler.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddFanexLogging(builder.Configuration)
    .AddQuartz(quartz =>
    {
        quartz.UseMicrosoftDependencyInjectionJobFactory();
        quartz.AddNotificationJobs(
            builder.Configuration.GetSection("ScheduleFunctions").Get<ScheduleFunction[]>(),
            Fanex.Logging.Logger.Log);
    })
    .AddQuartzServer(options => options.WaitForJobsToComplete = true);

var app = builder.Build();
app.MapGet("/", () => "Telegram bot scheduler!!!");

await app.RunAsync("http://0.0.0.0:5000");
