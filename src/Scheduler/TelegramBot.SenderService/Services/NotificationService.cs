using System.Text;
using Fanex.Data.Repository;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.Services;

public interface INotificationService
{
    Task<(IEnumerable<Notification>, IEnumerable<LastNotification>)> GetNotifications(string environment);

    Task<Result> UpdateNotificationStatus(IEnumerable<NotificationStatusParam> notifications, IEnumerable<LastNotification> lastNotifications, string environment);

    Task<Result> UpdateNotificationStatus(NotificationStatusParam statusParam, string environment);
}

public class NotificationService : INotificationService
{
    private readonly IDynamicRepository dynamicRepository;

    public NotificationService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public Task<(IEnumerable<Notification>, IEnumerable<LastNotification>)> GetNotifications(string environment)
        => dynamicRepository.FetchMultipleAsync<Notification, LastNotification>(new GetNotificationCriteria(environment));

    public Task<Result> UpdateNotificationStatus(NotificationStatusParam statusParam, string environment)
        => UpdateNotificationStatus(new List<NotificationStatusParam> { statusParam }, Enumerable.Empty<LastNotification>(), environment);

    public async Task<Result> UpdateNotificationStatus(
        IEnumerable<NotificationStatusParam> notifications,
        IEnumerable<LastNotification> lastNotifications,
        string environment)
    {
        var command = new UpdateNotificationStatusCommand(
            BuildXmlData(notifications),
            BuildXmlLastNotification(lastNotifications),
            environment);

        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildXmlData(IEnumerable<NotificationStatusParam> notificationStatus)
    {
        var xmlData = new StringBuilder();
        xmlData.Append("<root>");

        foreach (var notification in notificationStatus)
        {
            xmlData.Append(notification.ToXml());
        }

        xmlData.Append("</root>");
        return xmlData.ToString();
    }

    private static string BuildXmlLastNotification(IEnumerable<LastNotification> lastNotifications)
    {
        var xmlLastNotification = new StringBuilder();
        xmlLastNotification.Append("<root>");

        foreach (var item in lastNotifications)
        {
            xmlLastNotification.Append(item.ToXml());
        }

        xmlLastNotification.Append("</root>");
        return xmlLastNotification.ToString();
    }
}
