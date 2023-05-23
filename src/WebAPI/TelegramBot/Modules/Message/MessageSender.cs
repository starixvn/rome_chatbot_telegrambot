using Fanex.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Modules.Message.Constants;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Message
{
    public interface IMessageSender
    {
        Task SendMessage(CustBotAccount account, string message, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(int userId, Messenger messenger, string message, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(CustBotAccount account, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(int userId, Messenger messenger, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(Messenger messenger, string message, InlineKeyboardMarkup replyMarkup = null, int messageId = 0);

        Task Alert(int botId, string message, string callbackId);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IBotClientFactory botClientFactory;
        private readonly IMessageLogger messageLogger;
        private readonly ILogger logger;

        public MessageSender(IBotClientFactory botClientFactory, IMessageLogger messageLogger, ILogger logger)
        {
            this.botClientFactory = botClientFactory;
            this.messageLogger = messageLogger;
            this.logger = logger;
        }

        public Task SendMessage(CustBotAccount account, string message, short functionId = 0, bool logToDatabase = true)
            => SendMessage(account.UserId, new Messenger(account.BotId, account.ChatId), message, functionId, logToDatabase);

        public async Task SendMessage(int userId, Messenger messenger, string message, short functionId = 0, bool logToDatabase = true)
        {
            var status = MessageSendStatus.Success;
            try
            {
                await SendMessage(messenger, message);
            }
            catch (Exception ex)
            {
                if (ex is ApiRequestException apiException)
                {
                    if (apiException.ErrorCode == TelegramAPIErrors.BAD_REQUEST && apiException.Message.Equals(TelegramAPIErrors.DuplicatedErrorMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        status = MessageSendStatus.Duplicated;
                        await logger.WarnAsync(string.Join(Environment.NewLine, $"ChatId: {messenger.ChatId}", apiException.Message));
                    }
                    else if (apiException.ErrorCode == TelegramAPIErrors.FORBIDDEN)
                    {
                        status = MessageSendStatus.BotBlocked;
                        await logger.WarnAsync(string.Join(Environment.NewLine, $"ChatId: {messenger.ChatId}", apiException.Message));
                    }
                    else
                    {
                        status = MessageSendStatus.Failure;
                        throw;
                    }
                }
                else
                {
                    status = MessageSendStatus.Failure;
                    throw;
                }

            }
            finally
            {
                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, userId, messenger, message, status, functionId);
                }
            }
        }

        public Task SendMessage(CustBotAccount account, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true)
            => SendMessage(account.UserId, new Messenger(account.BotId, account.ChatId), message, messageId, functionId, logToDatabase);

        public async Task SendMessage(int userId, Messenger messenger, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true)
        {
            var status = MessageSendStatus.Success;
            try
            {
                await SendMessage(messenger, message.Text, messageId: messageId, replyMarkup: message.ReplyMarkup);
            }
            catch (Exception ex)
            {
                if (ex is ApiRequestException apiException)
                {
                    if (apiException.ErrorCode == TelegramAPIErrors.BAD_REQUEST && apiException.Message.Equals(TelegramAPIErrors.DuplicatedErrorMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        status = MessageSendStatus.Duplicated;
                        await logger.WarnAsync(string.Join(Environment.NewLine, $"ChatId: {messenger.ChatId}", apiException.Message));
                    }
                    else if (apiException.ErrorCode == TelegramAPIErrors.FORBIDDEN)
                    {
                        status = MessageSendStatus.BotBlocked;
                        await logger.WarnAsync(string.Join(Environment.NewLine, $"ChatId: {messenger.ChatId}", apiException.Message));
                    }
                    else
                    {
                        status = MessageSendStatus.Failure;
                        throw;
                    }
                }
                else
                {
                    status = MessageSendStatus.Failure;
                    throw;
                }

            }
            finally
            {
                if (logToDatabase)
                {
                    await messageLogger.LogMessage(CommonId.BotUserId, userId, messenger, message, status, functionId);
                }
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

        public async Task Alert(int botId, string message, string callbackId)
        {
            var botClient = await botClientFactory.GetOrCreate(botId);
            await botClient.AnswerCallbackQueryAsync(callbackId, message);
        }
    }
}