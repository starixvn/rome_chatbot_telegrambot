using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Infrastructure.Message
{
    public interface ISenderService
    {
        Task SendMessage(long messengerBotId, long chatId, string message, ParseMode? parseMode = null, IReplyMarkup replyMarkup = null, bool logToDatabase = true);
    }

    public class SenderService : ISenderService
    {
        private readonly IBotClientFactory botClientFactory;

        public SenderService(IBotClientFactory botClientFactory)
        {
            this.botClientFactory = botClientFactory;
        }

        public async Task SendMessage(long messengerBotId, long chatId, string message, ParseMode? parseMode = null, IReplyMarkup replyMarkup = null, bool logToDatabase = true)
        {
            var botClient = botClientFactory.GetOrCreate(messengerBotId);
            await botClient.SendTextMessageAsync(
                parseMode: parseMode,
                chatId: chatId,
                text: message,
                replyMarkup: replyMarkup);
        }
    }
}