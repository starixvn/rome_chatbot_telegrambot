using System.Security;
using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.PersonalMessages;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Resources;

namespace TelegramBot.Importer.Services;

public interface IPersonalMessageService
{
    Task<Tuple<int, IEnumerable<PersonalMessage>>> GetNewMessages(int msgTargetId, int batchSize, bool isTest);

    string BuildMessage(string language, string currentDate, string content, string transId = "");

    Task<Result> Import(
        IEnumerable<ImportPersonalMessage> notifications,
        string environment,
        int maxMsgTargetId);
}

public class PersonalMessageService : IPersonalMessageService
{
    private readonly IDynamicRepository dynamicRepository;

    public PersonalMessageService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<Tuple<int, IEnumerable<PersonalMessage>>> GetNewMessages(int msgTargetId, int batchSize, bool isTest)
    {
        var command = new GetPersonalMessageCommand(msgTargetId, batchSize, isTest);
        var personalMessages = await dynamicRepository.FetchAsync<PersonalMessage>(command);

        return new Tuple<int, IEnumerable<PersonalMessage>>(command.MaxMsgTargetId, personalMessages);
    }

    public string BuildMessage(string language, string currentDate, string content, string transId = "")
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(BuildTitle(TemplateHelper.Get("PersonalMessage", language), currentDate))
            .Append(SafeMessageContent(content, transId));

        return messageBuilder.ToString();
    }

    private static string BuildTitle(string title, string date)
        => $"[{title} - {date}]";

    private static string SafeMessageContent(string content, string transId)
        => SecurityElement.Escape(transId.Length == 0 ? content : content.Replace("@@", transId));

    public async Task<Result> Import(
        IEnumerable<ImportPersonalMessage> notifications,
        string environment,
        int maxMsgTargetId)
    {
        var command = new ImportPersonalMessageCommand(BuildXMLData(notifications), environment, maxMsgTargetId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildXMLData(IEnumerable<ImportPersonalMessage> notifications)
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