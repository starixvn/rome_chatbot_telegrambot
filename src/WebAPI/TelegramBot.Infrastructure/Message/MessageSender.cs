using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Message.Constants;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageSender
    {
        Task SendMessageWithBackButton(CustBotMessenger messenger, Models.Message message, int messageId = 0, short backToActionId = -1, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(CustBotMessenger messenger, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(CustBotMessenger messenger, string message, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(int botId, long chatId, string message, InlineKeyboardMarkup replyMarkup = null, int messageId = 0);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IBotClientFactory botClientFactory;
        private readonly IMessageLogger messageLogger;
        private readonly IMessageBuilder messageBuilder;
        private readonly int NumberButtonInRow = 2;

        public MessageSender(IBotClientFactory botClientFactory, IMessageLogger messageLogger, IMessageBuilder messageBuilder)
        {
            this.botClientFactory = botClientFactory;
            this.messageLogger = messageLogger;
            this.messageBuilder = messageBuilder;
        }

        public async Task SendMessageWithBackButton(CustBotMessenger messenger, Models.Message message, int messageId = 0, short backToActionId = 0, short functionId = 0, bool logToDatabase = true)
            => await SendMessage(messenger, message, messageId, backToActionId, functionId, logToDatabase);

        public async Task SendMessage(CustBotMessenger messenger, Models.Message message, int messageId = 0, short functionId = 0, bool logToDatabase = true)
            => await SendMessage(messenger, message, messageId, -1, functionId, logToDatabase);

        public async Task SendMessage(CustBotMessenger messenger, string message, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                await SendMessage(messenger.BotId, messenger.ChatId, message);

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

        public async Task SendMessage(int botId, long chatId, string message, InlineKeyboardMarkup replyMarkup = null, int messageId = 0)
        {
            var botClient = botClientFactory.GetOrCreate(botId);

            if (messageId > 0)
            {
                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: messageId,
                    text: message,
                    replyMarkup: replyMarkup);
            }
            else
            {
                var result = await botClient.SendTextMessageAsync(
                    parseMode: ParseMode.Html,
                    chatId: chatId,
                    text: message,
                    replyMarkup: replyMarkup);
            }
        }

        private async Task SendMessage(CustBotMessenger messenger, Models.Message message, int messageId, short backToActionId = 0, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                var replyMarkup = await BuildReplyMarkup(messenger.Language, message.Buttons, backToActionId);
                await SendMessage(messenger.BotId, messenger.ChatId, message.Text, messageId: messageId, replyMarkup: replyMarkup);

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

        private async Task<InlineKeyboardMarkup> BuildReplyMarkup(string language, IList<MessageButton> buttons, short backToActionId = -1)
        {
            var hasBackButton = backToActionId >= 0;
            var numberOfRows = (buttons.Count % NumberButtonInRow > 0 ? (buttons.Count / NumberButtonInRow) + 1 : (buttons.Count / NumberButtonInRow)) + (hasBackButton ? 1 : 0);
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                if (hasBackButton && i == numberOfRows - 1)
                {
                    var text = await messageBuilder.BuildBackButton(language, backToActionId);
                    var callBackData = JsonConvert.SerializeObject(new { ActionId = backToActionId, Value = text });
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(text, callBackData)
                    };

                    break;
                }

                var actionIndex = i * NumberButtonInRow;
                if (actionIndex == buttons.Count - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex].Text, buttons[actionIndex].CallBackData)
                    };
                }
                else
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex].Text, buttons[actionIndex].CallBackData),
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex + 1].Text, buttons[actionIndex + 1].CallBackData)
                    };
                }
            }

            return buttonRows;
        }
    }
}