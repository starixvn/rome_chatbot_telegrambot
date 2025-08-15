using System.Diagnostics.CodeAnalysis;
using System.Text;
using Quartz;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Jobs;

namespace TelegramBot.SenderService.Extensions;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static void AddNotificationSenderJob(
        this IServiceCollectionQuartzConfigurator quartz,
        IConfiguration configuration,
        Fanex.Logging.ILogger logger)
    {
        var configSection = configuration.GetSection("NotificationSender");

        if (configSection.GetValue<bool>("IsActive"))
        {
            quartz.AddNotificationSenderJob(
                configSection.GetValue<string>("Name"),
                configSection.GetValue<string>("Environment"),
                configSection.GetValue<int>("NotificationChunkSize"),
                configSection.GetValue<string>("CronExpression"),
                logger);
        }
    }

    private static void AddNotificationSenderJob(
        this IServiceCollectionQuartzConfigurator quartz,
        string name,
        string environment,
        int notificationChunkSize,
        string cronExpression,
        Fanex.Logging.ILogger logger)
    {
        const string GroupName = "Notification";
        var jobKey = new JobKey(name, GroupName);

        quartz.AddJob<NotificationSenderJob>(opts => opts
            .WithIdentity(jobKey)
            .UsingJobData(JobDataKey.JobName, name)
            .UsingJobData(JobDataKey.Environment, environment)
            .UsingJobData(JobDataKey.NotificationChunkSize, notificationChunkSize));

        quartz.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{name}-Trigger", GroupName)
            .WithCronSchedule(cronExpression));

        logger.Info(BuildJobNameAndEnvironmentInfo(name, environment));
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