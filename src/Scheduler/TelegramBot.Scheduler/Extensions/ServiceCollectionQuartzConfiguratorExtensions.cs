using Quartz;
using TelegramBot.Scheduler.Configuration;
using TelegramBot.Scheduler.Constants;
using TelegramBot.Scheduler.Jobs;

namespace TelegramBot.Scheduler.Extensions;

public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static void AddNotificationJobs(
        this IServiceCollectionQuartzConfigurator quartz,
        ScheduleFunction[] functions,
        Fanex.Logging.ILogger logger)
        => functions
            .Where(f => f.GroupName.Equals("Notification", StringComparison.OrdinalIgnoreCase) && f.IsActive)
            .ToList()
            .ForEach(async item =>
            {
                await AddNotificationJobAsync(quartz, item, logger);
            });
    
    private static Task AddNotificationJobAsync(
        IServiceCollectionQuartzConfigurator quartz,
        ScheduleFunction function,
        Fanex.Logging.ILogger logger)
    {
        var jobKey = new JobKey(function.Name, function.GroupName);

        quartz.AddJob<NotificationJob>(opts => opts
            .WithIdentity(jobKey)
            .UsingJobData(JobDataKey.FunctionId, function.Id)
            .UsingJobData(JobDataKey.FunctionName, function.Name));

        quartz.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{function.Name}-Trigger", function.GroupName)
            .WithCronSchedule(function.CronExpression));

        return logger.InfoAsync(
            $"Schedule notification with ID and Name: {function.Id}, {function.Name} has been start up.");
    }
}