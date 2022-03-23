using Fanex.Logging;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBot.Common.Configurations;

namespace TelegramBot.Infrastructure.Message
{
    public interface ISenderService
    {
        Task SendMessage(long messengerBotId, long chatId, string message, bool logToDatabase = true);
    }

    public class SenderService : ISenderService
    {
        private readonly WebApiConfiguration webApiConfiguration;
        private readonly ILogger logger;
        private readonly IBotClientFactory botClientFactory;

        public SenderService(WebApiConfiguration webApiConfiguration, ILogger logger, IBotClientFactory botClientFactory)
        {
            this.webApiConfiguration = webApiConfiguration;
            this.logger = logger;
            this.botClientFactory = botClientFactory;
        }

        public async Task SendMessage(long messengerBotId, long chatId, string message, bool logToDatabase = true)
        {
            var botClient = botClientFactory.GetOrCreate(messengerBotId);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message);
        }
    }
}
