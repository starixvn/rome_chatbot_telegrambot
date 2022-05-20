using System.Security;
using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Criteria.PersonalMessages;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.PersonalMessages;

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
    private static readonly IDictionary<string, string> MessageTemplates = new Dictionary<string, string>
    {
        [LanguageCode.English] = "[Personal Message - {0}]\n{1}",
        [LanguageCode.ChineseTraditional] = "[個人訊息 - {0}]\n{1}",
        [LanguageCode.ChineseSimplified] = "[个人讯息 - {0}]\n{1}",
        [LanguageCode.Thai] = "[ข้อความส่วนตัว - {0}]\n{1}",
        [LanguageCode.Vietnamese] = "[Tin nhắn cá nhân - {0}]\n{1}"
    };
    
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
    {
        if (!MessageTemplates.TryGetValue(language, out var template))
        {
            template = MessageTemplates[LanguageCode.English];
        }

        return string.Format(
            template,
            DateTime.Today.ToSimpleDate(),
            SecurityElement.Escape(transId.Length == 0 ? content : content.Replace("@@", transId)));
    }

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