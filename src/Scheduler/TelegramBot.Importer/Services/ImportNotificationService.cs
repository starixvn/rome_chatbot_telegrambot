using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.Notifications;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Notifications;

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

    Task<Result> ImportProblemAccounts(
        IEnumerable<ProblemAccountNotification> notifications,
        string environment,
        int maxLogId);
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

    public async Task<Result> ImportProblemAccounts(
        IEnumerable<ProblemAccountNotification> notifications,
        string environment,
        int maxLogId)
    {
        var command = new ImportProblemAccountCommand(
            BuildXmlData(notifications),
            BuildXmlDataLog(notifications),
            environment,
            maxLogId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildXmlData(IEnumerable<Notification> notifications)
    {
        var xmlData = new StringBuilder();

        foreach (var item in notifications)
        {
            xmlData.Append(item.ToXml());
        }
            
        return BuildXmlRoot(xmlData);
    }

    private static string BuildXmlDataLog(IEnumerable<Notification> notifications)
    {
        var xmlDataLog = new StringBuilder();

        foreach (var item in notifications)
        {
            xmlDataLog.Append(item.ToLogXml());
        }
            
        return BuildXmlRoot(xmlDataLog);
    }

    private static string BuildXmlRoot(StringBuilder xmlBuilder)
        => $"<root>{xmlBuilder}</root>";
}