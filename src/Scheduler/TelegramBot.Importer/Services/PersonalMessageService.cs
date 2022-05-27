using System.Security;
using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.PersonalMessages;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Resources;

namespace TelegramBot.Importer.Services;

public interface IPersonalMessageService
{
    Task<Tuple<int, IEnumerable<PersonalMessage>>> GetNewMessages(int msgTargetId, int batchSize, bool isTest);

    string BuildMessage(string language, string content, string transId = "");

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

    public string BuildMessage(string language, string content, string transId = "")
        => string.Format(
            TemplateHelper.Get("PersonalMessage", language),
            DateTime.Today.ToSimpleDate(),
            TelegramHtmlFormat.NewLine,
            SafeMessageContent(content, transId));

    private static string SafeMessageContent(string content, string transId)
        => SecurityElement.Escape(transId.Length == 0 ? content : content.Replace("@@", transId));

    public async Task<Result> Import(
        IEnumerable<ImportPersonalMessage> notifications,
        string environment,
        int maxMsgTargetId)
    {
        var command = new ImportPersonalMessageCommand(BuildPersonalMessageXMLData(notifications), environment, maxMsgTargetId);
        await dynamicRepository.ExecuteAsync(command);

        return new Result(command.ErrCode, command.ErrMsg);
    }

    private static string BuildPersonalMessageXMLData(IEnumerable<ImportPersonalMessage> notifications)
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