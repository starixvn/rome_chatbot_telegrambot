using Quartz;
using Telegram.Bot.Exceptions;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Jobs;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Jobs;

public class RetryJobTests
{
    private readonly int MaxRetries = 5;
    private readonly IBotService botService;
    private readonly INotificationService notificationService;
    private readonly RetryJob retryJob;
    private readonly IJobExecutionContext context;

    public RetryJobTests()
    {
        botService = Substitute.For<IBotService>();
        notificationService = Substitute.For<INotificationService>();
        retryJob = new RetryJob(botService, notificationService);
        context = Substitute.For<IJobExecutionContext>();
    }

    [Theory, AutoData]
    public async Task Execute_Always_CallsSendMessage(int botId, long chatId, int notificationId, string message)
    {
        // Arrange
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "botId", botId },
            { "chatId", chatId },
            { "notificationId", notificationId },
            { "message", message },
            { "numberOfRetries", 1 },
            { "Environment", "UAT" },
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));

        // Act
        await retryJob.Execute(context);

        // Assert
        await botService.Received().SendMessage(botId, chatId, message);
        await notificationService.Received().UpdateNotificationStatus(Arg.Is<NotificationStatusParam>(
            p => p.NotificationId == notificationId && p.Status == NotificationStatus.Completed && p.Retry == 1), "UAT");
    }

    [Theory, AutoData]
    public async Task Execute_HasExceptionAndNumberOfRetriesIsMax_CallsUpdateNotificationStatus(int botId, long chatId, int notificationId, string message)
    {
        // Arrange
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "botId", botId },
            { "chatId", chatId },
            { "notificationId", notificationId },
            { "message", message },
            { "numberOfRetries", MaxRetries },
            { "Environment", "UAT" },
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        botService.SendMessage(botId, chatId, message).Returns(Task.FromException(new Exception("errMsg")));

        // Act
        await retryJob.Execute(context);

        // Assert
        await notificationService.Received().UpdateNotificationStatus(Arg.Is<NotificationStatusParam>(
            p => p.NotificationId == notificationId && p.Status == NotificationStatus.Fail && p.Retry == MaxRetries && p.ErrorMessage == "errMsg"), "UAT");
    }

    [Theory, AutoData]
    public async Task Execute_HasExceptionAndErrorCodeIsIgnore_CallsUpdateNotificationStatus(int botId, long chatId, int notificationId, string message)
    {
        // Arrange
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "botId", botId },
            { "chatId", chatId },
            { "notificationId", notificationId },
            { "message", message },
            { "numberOfRetries", 2 },
            { "Environment", "UAT" },
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        botService.SendMessage(botId, chatId, message).Returns(Task.FromException(new ApiRequestException("errMsg", 400)));

        // Act
        await retryJob.Execute(context);

        // Assert
        await notificationService.Received().UpdateNotificationStatus(Arg.Is<NotificationStatusParam>(
            p => p.NotificationId == notificationId && p.Status == NotificationStatus.Fail && p.Retry == 2 && p.ErrorMessage == "errMsg"), "UAT");
    }

    [Theory, AutoData]
    public async Task Execute_HasExceptionInOtherCase_ThrowsJobExecutionException(int botId, long chatId, int notificationId, string message)
    {
        // Arrange
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "botId", botId },
            { "chatId", chatId },
            { "notificationId", notificationId },
            { "message", message },
            { "numberOfRetries", 2 },
            { "Environment", "UAT" },
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        botService.SendMessage(botId, chatId, message).Returns(Task.FromException(new Exception("errMsg")));

        // Assert
        await Assert.ThrowsAsync<JobExecutionException>(async () => await retryJob.Execute(context));
    }
}