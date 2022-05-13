using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Notifications.Shared.Criteria;
using TelegramBot.Notifications.Shared.Models;
using TelegramBot.Notifications.Shared.Params;

namespace TelegramBot.Notifications.Shared.Services;

public interface INotificationService
{
    Task<Result> Import(IEnumerable<ImportPersonalMessageParam> notifications, string environment, int maxMsgTargetId);
}

public class NotificationService : INotificationService
{
    private readonly IDynamicRepository dynamicRepository;

    public NotificationService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<Result> Import(IEnumerable<ImportPersonalMessageParam> notifications, string environment, int maxMsgTargetId)
    {
        var command = new ImportPersonalMessageCommand(BuildPersonalMessageXMLData(notifications), environment, maxMsgTargetId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildPersonalMessageXMLData(IEnumerable<ImportPersonalMessageParam> notifications)
    {
        var xmlData = new StringBuilder();
        xmlData.Append("<root>");

        foreach (var item in notifications)
        {
            xmlData.Append(item.ToXml());
        }
            
        xmlData.Append("</root>");

        return xmlData.ToString();
    }
}
