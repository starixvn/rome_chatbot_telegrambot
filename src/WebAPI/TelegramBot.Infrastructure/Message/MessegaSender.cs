using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Message.Constants;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageSender
    {
        Task SendMessage(int botId, long chatId, string message, IReplyMarkup replyMarkup = null);

        Task SendMessage(CustBotMessenger messenger, string message, IReplyMarkup replyMarkup = null, short functionId = 0, bool logToDatabase = true);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IBotClientFactory botClientFactory;
        private readonly IMessageLogger messageLogger;

        public MessageSender(IBotClientFactory botClientFactory, IMessageLogger messageLogger)
        {
            this.botClientFactory = botClientFactory;
            this.messageLogger = messageLogger;
        }

        public async Task SendMessage(CustBotMessenger messenger, string message, IReplyMarkup replyMarkup = null, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                await SendMessage(messenger.BotId, messenger.ChatId, message, replyMarkup);

                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, messenger.UserId, messenger.ChatId, messenger.BotId, message, MessageSendStatus.Success, functionId);
                }
            }
            catch
            {
                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, messenger.UserId, messenger.ChatId, messenger.BotId, message, MessageSendStatus.Failure, functionId);
                }

                throw;
            }
        }

        public async Task SendMessage(int botId, long chatId, string message, IReplyMarkup replyMarkup = null)
        {
            var botClient = botClientFactory.GetOrCreate(botId);

            await botClient.SendTextMessageAsync(
                parseMode: ParseMode.Html,
                chatId: chatId,
                text: message,
                replyMarkup: replyMarkup);
        }
    }
}