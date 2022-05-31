using Quartz;
using TelegramBot.SenderService.Extensions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((host, services) =>
    {
        services.AddFanexDataWithSqlServer(host.Configuration)
        .AddFanexCachingMemory()
        .AddFanexLogging(host.Configuration)
        .AddONEBookUM(host.Configuration)
        .AddServices(host.Configuration)
        .AddQuartz(quartz =>
        {
            quartz.UseMicrosoftDependencyInjectionJobFactory();
            quartz.AddNotificationSenderJob(host.Configuration, Fanex.Logging.Logger.Log);
        })
        .AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }).ConfigureAppConfiguration((host, builder) => builder.UseONEBookUM(host.Configuration));

var app = builder.Build();
await app.RunAsync();
