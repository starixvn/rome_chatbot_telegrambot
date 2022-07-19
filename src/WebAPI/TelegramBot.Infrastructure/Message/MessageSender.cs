using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Message.Constants;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageSender
    {
        Task SendMessage(int userId, Messenger messenger, string message, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(int userId, Messenger messenger, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(Messenger messenger, string message, InlineKeyboardMarkup replyMarkup = null, int messageId = 0);

        Task Alert(Messenger messenger, string message, string callbackId);
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

        public async Task SendMessage(int userId, Messenger messenger, string message, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                await SendMessage(messenger, message);

                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, userId, messenger, message, MessageSendStatus.Success, functionId);
                }
            }
            catch
            {
                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, userId, messenger, message, MessageSendStatus.Failure, functionId);
                }

                throw;
            }

        }

        public async Task SendMessage(int userId, Messenger messenger, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                await SendMessage(messenger, message.Text, messageId: messageId, replyMarkup: message.ReplyMarkup);

                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, userId, messenger, message, MessageSendStatus.Success, functionId);
                }
            }
            catch
            {
                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, userId, messenger, message, MessageSendStatus.Failure, functionId);
                }

                throw;
            }
        }

        public async Task SendMessage(Messenger messenger, string message, InlineKeyboardMarkup replyMarkup = null, int messageId = 0)
        {
            var botClient = await botClientFactory.GetOrCreate(messenger.BotId);

            if (messageId > 0)
            {
                await botClient.EditMessageTextAsync(
                    chatId: messenger.ChatId,
                    messageId: messageId,
                    text: message,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    parseMode: ParseMode.Html,
                    chatId: messenger.ChatId,
                    text: message,
                    replyMarkup: replyMarkup);
            }
        }

        public async Task Alert(Messenger messenger, string message, string callbackId)
        {
            var botClient = await botClientFactory.GetOrCreate(messenger.BotId);
            await botClient.AnswerCallbackQueryAsync(callbackId, message);
        }
    }
}