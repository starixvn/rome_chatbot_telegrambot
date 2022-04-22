using Quartz;
using TelegramBot.Scheduler.Configuration;
using TelegramBot.Scheduler.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services
            .AddFanexLogging(configuration)
            .AddQuartz(quartz =>
            {
                quartz.UseMicrosoftDependencyInjectionJobFactory();
                quartz.AddNotificationJobs(
                    configuration.GetSection("ScheduleFunctions").Get<ScheduleFunction[]>(),
                    Fanex.Logging.Logger.Log);
            })
            .AddQuartzHostedService(quartz => quartz.WaitForJobsToComplete = true);
    })
    .Build();

await host.RunAsync();
