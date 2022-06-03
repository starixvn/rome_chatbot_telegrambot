using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria;
using TelegramBot.Importer.Models;

namespace TelegramBot.Importer.Services;

public class NotificationServiceBase
{
    protected readonly IDynamicRepository dynamicRepository;

    protected NotificationServiceBase(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    protected async Task<Result> ExecuteNonQuery(NonQueryCommandBase command)
    {
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    protected static string BuildXmlData(IEnumerable<Notification> notifications)
    {
        var xmlData = new StringBuilder();

        foreach (var item in notifications)
        {
            xmlData.Append(item.ToXml());
        }

        return BuildXmlRoot(xmlData);
    }

    protected static string BuildXmlDataLog(IEnumerable<Notification> notifications)
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