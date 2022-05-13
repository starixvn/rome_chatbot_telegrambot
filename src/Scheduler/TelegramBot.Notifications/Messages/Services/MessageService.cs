using Alpha.Notifications.Messages.Criteria;
using Alpha.Notifications.Messages.Models;
using Fanex.Data.Repository;

namespace TelegramBot.Notifications.Messages.Services;

public interface IMessageService
{
    Task<Tuple<int, IEnumerable<PersonalMessage>>> GetPersonalMessages(int msgTargetId, int batchSize, bool isTest);
}

public class MessageService : IMessageService
{
    private readonly IDynamicRepository dynamicRepository;

    public MessageService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<Tuple<int, IEnumerable<PersonalMessage>>> GetPersonalMessages(int msgTargetId, int batchSize, bool isTest)
    {
        var command = new GetPersonalMessageCommand(msgTargetId, batchSize, isTest);
        var personalMessages = await dynamicRepository.FetchAsync<PersonalMessage>(command);

        return new Tuple<int, IEnumerable<PersonalMessage>>(command.MaxMsgTargetId, personalMessages);
    }
}