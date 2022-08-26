using Quartz;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Services;

public class MessageSenderTests
{
    private readonly IBotService botService;
    private readonly ISchedulerFactory schedulerFactory;
    private readonly IJobListener retryJobListener;
    private readonly MessageSender messageSender;

    public MessageSenderTests()
    {
        botService = Substitute.For<IBotService>();
        schedulerFactory = Substitute.For<ISchedulerFactory>();
        retryJobListener = Substitute.For<IJobListener>();
        messageSender = new MessageSender(botService, schedulerFactory, retryJobListener);
    }

    [Theory, AutoData]
    public async Task SendNotification_Always_CallsSendMessage(Notification notification)
    {
        // Act
        await messageSender.SendNotification(notification);

        // Assert
        await botService.Received().SendMessage(notification.BotId, notification.ChatId, notification.Message);
    }

    [Theory, AutoData]
    public async Task RetrySendNotification_Always_CallsScheduleJob(Notification notification, string environment)
    {
        var scheduler = Substitute.For<IScheduler>();
        schedulerFactory.GetScheduler().Returns(Task.FromResult(scheduler));

        // Act
        await messageSender.RetrySendNotification(notification, environment);

        // Assert
        await scheduler.Received().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
    }
}