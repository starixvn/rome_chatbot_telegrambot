using System.Threading.Tasks;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Message.criteria;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageLogger
    {
        Task LogMessage(int sender, int receiver, long chatId, int botId, string message, byte status = 0, short functionId = 0);
    }

    public class MessageLogger : IMessageLogger
    {
        private readonly IDynamicRepository dynamicRepository;

        public MessageLogger(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task LogMessage(int sender, int receiver, long chatId, int botId, string message, byte status = 0, short functionId = 0)
        {
            var command = new CustBotMessageInsCriteria(sender, receiver, chatId, botId, functionId, message, status);
            await dynamicRepository.ExecuteAsync(command);
        }
    }
}