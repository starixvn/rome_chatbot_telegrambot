using Fanex.Data.Repository;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly NotificationService notificationService;

    public NotificationServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        notificationService = new NotificationService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetNotifications_Always_CallsFetchMultipleAsync(string environment)
    {
        // Act
        await notificationService.GetNotifications(environment);

        // Assert
        await dynamicRepository.Received().FetchMultipleAsync<Notification, LastNotification>(Arg.Is<GetNotificationCriteria>(c => c.Environment == environment));
    }

    [Theory, AutoData]
    public async Task UpdateNotificationStatus_Always_CallsExecuteAsync(string environment)
    {
        // Arrange
        var statusParam = new NotificationStatusParam(1, NotificationStatus.Completed);
        var xmlData = $"<root><r NotificationId=\"1\" Status=\"Completed\" ErrMsg=\"\" Retry=\"{0}\" /></root>";

        // Act
        await notificationService.UpdateNotificationStatus(statusParam, environment);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<UpdateNotificationStatusCommand>(c => c.Environment == environment && c.XmlData == xmlData));
    }

    [Theory, AutoData]
    public async Task UpdateNotificationStatus_Always_CallsExecuteAsyncWithCorrectCommand(string environment)
    {
        // Arrange
        IEnumerable<NotificationStatusParam> notifications = new List<NotificationStatusParam> { new NotificationStatusParam(1, NotificationStatus.Completed) };
        var xmlData = $"<root><r NotificationId=\"1\" Status=\"Completed\" ErrMsg=\"\" Retry=\"{0}\" /></root>";
        IEnumerable<LastNotification> lastNotifications = new List<LastNotification>{
            new LastNotification(1,"SystemAnnouncement")
        };
        var xmlLastNotification = "<root><r LastnotificationId=\"1\" FunctionKey=\"SystemAnnouncement\"/></root>";

        // Act
        await notificationService.UpdateNotificationStatus(notifications, lastNotifications, environment);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<UpdateNotificationStatusCommand>(
            c => c.Environment == environment
            && c.XmlData == xmlData
            && c.XmlLastNotification == xmlLastNotification));
    }
}