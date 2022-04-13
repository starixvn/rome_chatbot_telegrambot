using Fanex.Logging;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Message
{
    public interface ISenderService
    {
        Task SendMessage(long messengerBotId, long chatId, string message, IReplyMarkup replyMarkup = null, bool logToDatabase = true);
    }

    public class SenderService : ISenderService
    {
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly ILogger logger;
        private readonly IBotClientFactory botClientFactory;

        public SenderService(WebAPIConfiguration webAPIConfiguration, ILogger logger, IBotClientFactory botClientFactory)
        {
            this.webAPIConfiguration = webAPIConfiguration;
            this.logger = logger;
            this.botClientFactory = botClientFactory;
        }

        public async Task SendMessage(long messengerBotId, long chatId, string message, IReplyMarkup replyMarkup = null, bool logToDatabase = true)
        {
            var botClient = botClientFactory.GetOrCreate(messengerBotId);
            await botClient.SendTextMessageAsync(
                parseMode: ParseMode.Html,
                chatId: chatId,
                text: message,
                replyMarkup: replyMarkup);
        }
    }
}
