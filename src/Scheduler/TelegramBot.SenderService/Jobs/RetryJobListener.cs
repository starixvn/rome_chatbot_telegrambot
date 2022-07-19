using Quartz;
using TelegramBot.SenderService.Constants;
namespace TelegramBot.SenderService.Jobs;

public class RetryJobListener : IJobListener
{
    private readonly Fanex.Logging.ILogger logger;

    public RetryJobListener(Fanex.Logging.ILogger logger)
    {
        this.logger = logger;
    }

    private const int RetryInterval = 10;

    public string Name => "RetryJobListener";

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var numberRetries = context.JobDetail.JobDataMap.GetIntValue(JobDataKey.NumberOfRetries);
        context.JobDetail.JobDataMap.Put(JobDataKey.NumberOfRetries, ++numberRetries);

        return Task.CompletedTask;
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        if (jobException == null)
        {
            return;
        }

        var numberRetries = context.JobDetail.JobDataMap.GetIntValue(JobDataKey.NumberOfRetries);

        var triggerBuilder = TriggerBuilder
            .Create()
            .WithIdentity(context.Trigger.Key)
            .StartAt(DateTime.Now.AddSeconds(RetryInterval * numberRetries))
            .ForJob(context.JobDetail);

        var trigger = triggerBuilder.WithSimpleSchedule(x => x.WithRepeatCount(0)).Build();

        context.JobDetail.JobDataMap.Put(JobDataKey.NumberOfRetries, numberRetries);

        await logger.InfoAsync($"Job with ID and type: {context.JobDetail.Key}, {context.JobDetail.JobType}" +
            $" has thrown the exception: {jobException}. Running again in {RetryInterval * numberRetries} seconds.");

        await context.Scheduler.UnscheduleJob(context.Trigger.Key, cancellationToken);
        await context.Scheduler.ScheduleJob(context.JobDetail, trigger, cancellationToken);
    }
}