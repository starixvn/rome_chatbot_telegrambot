using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.Notifications;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.PersonalMessages;

namespace TelegramBot.Importer.Services;

public interface IImportNotificationService
{
    Task<Result> ImportPersonalMessages(
        IEnumerable<PersonalMessageNotification> notifications,
        string environment,
        int maxMsgTargetId);

    Task<Result> ImportYesterdayWinLoss(
        IEnumerable<Notification> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId);
}

public class ImportNotificationService : IImportNotificationService
{
    private readonly IDynamicRepository dynamicRepository;

    public ImportNotificationService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<Result> ImportPersonalMessages(
        IEnumerable<PersonalMessageNotification> notifications,
        string environment,
        int maxMsgTargetId)
    {
        var command = new ImportPersonalMessageCommand(BuildXmlData(notifications), environment, maxMsgTargetId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    public async Task<Result> ImportYesterdayWinLoss(
        IEnumerable<Notification> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId)
    {
        var command = new ImportYesterdayWinLossCommand(
            BuildXmlData(notifications),
            winLossDate,
            environment,
            maxUserId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildXmlData(IEnumerable<Notification> notifications)
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