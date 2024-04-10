using System.Diagnostics.CodeAnalysis;
using System.Text;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;

namespace TelegramBot.ImporterService.Extensions;

[ExcludeFromCodeCoverage]
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
                var jobType = Type.GetType(
                    $"TelegramBot.Importer.Jobs.{job.Name}Job, TelegramBot.Importer", true);
                await quartz.AddNotificationJob(jobType, job, logger);
            });

    private static Task AddNotificationJob(
        this IServiceCollectionQuartzConfigurator quartz,
        Type jobType,
        ScheduleJob job,
        Fanex.Logging.ILogger logger)
    {
        const string GroupName = "Notification";
        var jobKey = new JobKey(job.Name, GroupName);

        quartz.AddJob(jobType, jobKey, opts => opts
            .UsingJobData(JobDataKey.JobName, job.Name)
            .UsingJobData(JobDataKey.SubAccPermission, job.SubAccPermission)
            .UsingJobData(JobDataKey.Environment, job.Environment)
            .UsingJobData(JobDataKey.IsGetTestData, job.IsGetTestData)
            .UsingJobData(JobDataKey.SiteDBs, job.SiteDBs)
            .UsingJobData(JobDataKey.MonitorEnvironment, job.MonitorEnvironment));

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