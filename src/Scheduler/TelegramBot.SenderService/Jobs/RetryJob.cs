using Quartz;
using Telegram.Bot.Exceptions;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.Jobs;

public class RetryJob : IJob
{
    private readonly int MaxRetries = 5;
    private readonly IBotService botService;
    private readonly INotificationService notificationService;

    public RetryJob(IBotService botService, INotificationService notificationService)
    {
        this.botService = botService;
        this.notificationService = notificationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var botId = dataMap.GetInt(JobDataKey.BotId);
        var chatId = dataMap.GetLong(JobDataKey.ChatId);
        var notificationId = dataMap.GetInt(JobDataKey.NotificationId);
        var message = dataMap.GetString(JobDataKey.Message);
        var numberOfRetries = dataMap.GetInt(JobDataKey.NumberOfRetries);
        var environment = dataMap.GetString(JobDataKey.Environment);

        try
        {
            await botService.SendMessage(botId, chatId, message);

            await notificationService.UpdateNotificationStatus(new NotificationStatusParam(notificationId, NotificationStatus.Completed, Retry: numberOfRetries), environment);
        }
        catch (Exception ex)
        {
            if (numberOfRetries == MaxRetries || (ex is ApiRequestException apiRequestException && TelegramError.IgnoredErrorCodes.Contains(apiRequestException.ErrorCode)))
            {
                await notificationService.UpdateNotificationStatus(new NotificationStatusParam(notificationId, NotificationStatus.Fail, ex.Message, numberOfRetries), environment);
            }
            else
            {
                throw new JobExecutionException(ex.Message);
            }
        }
    }
}
