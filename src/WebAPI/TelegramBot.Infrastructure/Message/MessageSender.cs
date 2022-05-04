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
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageSender
    {
        Task SendMessageWithCancelButtion(CustBotMessenger messenger, string message, short parentActionId, short functionId = 0, bool logToDatabase = true);

        Task SendMessageWithBackToMainMenuButton(CustBotMessenger messenger, string message, IList<MessageButton> buttons = null, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(CustBotMessenger messenger, string message, IList<MessageButton> buttons = null, short functionId = 0, bool logToDatabase = true);

        Task SendMessage(int botId, long chatId, string message, IReplyMarkup replyMarkup = null);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IBotClientFactory botClientFactory;
        private readonly IMessageLogger messageLogger;
        private readonly int NumberButtonInRow = 2;

        public MessageSender(IBotClientFactory botClientFactory, IMessageLogger messageLogger)
        {
            this.botClientFactory = botClientFactory;
            this.messageLogger = messageLogger;
        }

        public async Task SendMessageWithCancelButtion(CustBotMessenger messenger, string message, short parentActionId, short functionId = 0, bool logToDatabase = true)
        {
            await SendMessage(messenger, message, new List<MessageButton> { BuildCancelButtion(messenger.Language, parentActionId) }, false, functionId, logToDatabase);
        }

        public async Task SendMessageWithBackToMainMenuButton(CustBotMessenger messenger, string message, IList<MessageButton> buttons = null, short functionId = 0, bool logToDatabase = true)
        {
            await SendMessage(messenger, message, buttons, true, functionId, logToDatabase);
        }

        public async Task SendMessage(CustBotMessenger messenger, string message, IList<MessageButton> buttons = null, short functionId = 0, bool logToDatabase = true)
        {
            await SendMessage(messenger, message, buttons, false, functionId, logToDatabase);
        }

        private async Task SendMessage(CustBotMessenger messenger, string message, IList<MessageButton> buttons = null, bool withBackToMainMenuButton = false, short functionId = 0, bool logToDatabase = true)
        {
            IReplyMarkup replyMarkup = null;

            try
            {
                if (buttons != null)
                {
                    replyMarkup = BuildReplyMarkup(messenger.Language, buttons, withBackToMainMenuButton);
                }

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

        private MessageButton BuildCancelButtion(string language, short parentActionId)
        {
            return new MessageButton(ResourceHelper.Get(nameof(SharedResource.Cancel)), JsonConvert.SerializeObject(new { IsCancel = true, ActionId = parentActionId }));
        }
    }
}