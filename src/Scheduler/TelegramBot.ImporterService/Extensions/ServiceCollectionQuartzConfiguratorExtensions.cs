using System.Text;
using Quartz;
using TelegramBot.Notifications.Jobs;
using TelegramBot.Notifications.Shared.Configuration;
using TelegramBot.Notifications.Shared.Constants;

namespace TelegramBot.ImporterService.Extensions;

public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static void AddScheduleJobs(
        this IServiceCollectionQuartzConfigurator quartz,
        IConfiguration configuration,
        Fanex.Logging.ILogger logger)
        => configuration.GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()
            .Where(job => job.IsActive)
            .ToList()
            .ForEach(async job =>
            {
                if (job.Name.Equals("PersonalMessage", StringComparison.OrdinalIgnoreCase))
                {
                    await quartz.AddNotificationJob<PersonalMessageJob>(job, logger);
                }
            });

    private static Task AddNotificationJob<T>(
        this IServiceCollectionQuartzConfigurator quartz,
        ScheduleJob job,
        Fanex.Logging.ILogger logger) where T : IJob
    {
        const string GroupName = "Notification";
        var jobKey = new JobKey(job.Name, GroupName);

        quartz.AddJob<T>(opts => opts
            .WithIdentity(jobKey)
            .UsingJobData(JobDataKey.JobName, job.Name)
            .UsingJobData(JobDataKey.SubAccPermission, job.SubAccPermission)
            .UsingJobData(JobDataKey.Environment, job.Environment)
            .UsingJobData(JobDataKey.IsGetTestData, job.IsGetTestData));

        quartz.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{job.Name}-Trigger", GroupName)
            .WithCronSchedule(job.CronExpression));

        return logger.InfoAsync(BuildJobNameAndEnvironmentInfo(job.Name, job.Environment));
    }

    private static string BuildJobNameAndEnvironmentInfo(string jobName, string environment)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder.Append("Schedule job with name ").Append(jobName);

        if (!string.IsNullOrWhiteSpace(environment))
        {
            messageBuilder.Append(" and environment ").Append(environment);
        }

        messageBuilder.Append(" has been start up.");
        return messageBuilder.ToString();
    }
}