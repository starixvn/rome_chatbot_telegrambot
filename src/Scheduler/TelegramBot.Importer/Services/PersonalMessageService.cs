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

    Task<Result> Import(
        IEnumerable<PersonalMessageNotification> notifications,
        string environment,
        int maxMsgTargetId);
}

public class PersonalMessageService : NotificationServiceBase, IPersonalMessageService
{
    public PersonalMessageService(IDynamicRepository dynamicRepository)
        : base(dynamicRepository)
    {
    }

    public async Task<Tuple<int, IEnumerable<PersonalMessage>>> GetNewMessages(int msgTargetId, int batchSize, bool isTest)
    {
        var command = new GetPersonalMessageCommand(msgTargetId, batchSize, isTest);
        var personalMessages = await dynamicRepository.FetchAsync<PersonalMessage>(command);

        return new Tuple<int, IEnumerable<PersonalMessage>>(command.MaxMsgTargetId, personalMessages);
    }

    public Task<Result> Import(
        IEnumerable<PersonalMessageNotification> notifications,
        string environment,
        int maxMsgTargetId)
        => ExecuteNonQuery(new ImportPersonalMessageCommand(
            BuildXmlData(notifications),
            environment,
            maxMsgTargetId));
}