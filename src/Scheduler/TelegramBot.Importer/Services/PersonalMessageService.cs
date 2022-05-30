using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.PersonalMessages;
using TelegramBot.Importer.Models.PersonalMessages;

namespace TelegramBot.Importer.Services;

public interface IPersonalMessageService
{
    Task<Tuple<int, IEnumerable<PersonalMessage>>> GetNewMessages(int msgTargetId, int batchSize, bool isTest);
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
}