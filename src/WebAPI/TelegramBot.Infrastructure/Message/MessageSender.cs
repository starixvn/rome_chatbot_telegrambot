using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Caching;
using Fanex.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Message.Constants;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageSender
    {
        Task SendMessageWithBackToMainMenuButton(CustBotMessenger messenger, Models.Message message, short functionId = 0, bool logToDatabase = true, int messageId = 0);

        Task SendMessageWithCancelButton(CustBotMessenger messenger, string message, short backToActionId, short functionId = 0, bool logToDatabase = true, int messageId = 0);

        Task SendMessage(CustBotMessenger messenger, Models.Message message, short functionId = 0, bool logToDatabase = true, int messageId = 0);

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

        public async Task SendMessageWithBackToMainMenuButton(CustBotMessenger messenger, Models.Message message, short functionId = 0, bool logToDatabase = true, int messageId = 0)
            => await SendMessage(messenger, message, messageId, true, functionId, logToDatabase);

        public async Task SendMessage(CustBotMessenger messenger, Models.Message message, short functionId = 0, bool logToDatabase = true, int messageId = 0)
            => await SendMessage(messenger, message, messageId, false, functionId, logToDatabase);

        public async Task SendMessageWithCancelButton(CustBotMessenger messenger, string message, short backToActionId, short functionId = 0, bool logToDatabase = true, int messageId = 0)
            => await SendMessage(
                messenger,
                new Message.Models.Message(message, new List<MessageButton> { messageBuilder.BuildCancelButton(messenger.Language, backToActionId) }),
                functionId,
                logToDatabase, messageId);

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
                    replyMarkup: replyMarkup
                );
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

        private async Task SendMessage(CustBotMessenger messenger, Models.Message message, int messageId, bool withBackToMainMenuButton = false, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                var replyMarkup = BuildReplyMarkup(messenger.Language, message.Buttons, withBackToMainMenuButton);
                await SendMessage(messenger.BotId, messenger.ChatId, message.Text, replyMarkup, messageId);

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

        private InlineKeyboardMarkup BuildReplyMarkup(string language, IList<MessageButton> buttons, bool withBackToMainMenuButton = false)
        {
            var numberOfRows = buttons.Count % NumberButtonInRow > 0 ? (buttons.Count / NumberButtonInRow) + 1 : (buttons.Count / NumberButtonInRow);
            numberOfRows = withBackToMainMenuButton ? numberOfRows + 1 : numberOfRows;
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                if (withBackToMainMenuButton && i == numberOfRows - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            ResourceHelper.Get(nameof(SharedResource.BackToMainMenu),language),
                            JsonConvert.SerializeObject(new {IsBackToMainMenu=true}))
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