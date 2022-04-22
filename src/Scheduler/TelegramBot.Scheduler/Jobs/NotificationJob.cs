using Quartz;
using TelegramBot.Scheduler.Constants;

namespace TelegramBot.Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class NotificationJob : IJob
{
    private readonly Fanex.Logging.ILogger logger;

    public NotificationJob(Fanex.Logging.ILogger logger)
    {
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            var functionId = dataMap.GetIntValue(JobDataKey.FunctionId);
            var functionnName = dataMap.GetString(JobDataKey.FunctionName);

            await logger.InfoAsync($"Job with ID and Name: {functionId}, {functionnName} start to run.");

            //var functionMessages = await messageService.GetFunctionMessages(functionId);

            // Parallel.ForEach(functionMessages, async (functionMessage) =>
            // {
            //     await messageSender.SendMessageToUser(
            //         functionMessage.UserId,
            //         functionId,
            //         functionMessage.Message,
            //         functionMessage.SelfdestructionPeriod);
            // });

            await logger.InfoAsync($"Job with ID and Name: {functionId}, {functionnName} run completed.");
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(ex.Message, ex);
        }
    }
}