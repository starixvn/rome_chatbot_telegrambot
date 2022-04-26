using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Message.Constants;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Message
{
    public interface ISenderService
    {
        Task SendMessage(int botId, long chatId, string message, IReplyMarkup replyMarkup = null);

        Task SendMessage(CustBotMessenger messenger, string message, IReplyMarkup replyMarkup = null, short functionId = 0, bool logToDatabase = true);
    }

    public class SenderService : ISenderService
    {
        private readonly IBotClientFactory botClientFactory;
        private readonly ILoggerService loggerService;

        public SenderService(IBotClientFactory botClientFactory, ILoggerService loggerService)
        {
            this.botClientFactory = botClientFactory;
            this.loggerService = loggerService;
        }

        public async Task SendMessage(CustBotMessenger messenger, string message, IReplyMarkup replyMarkup = null, short functionId = 0, bool logToDatabase = true)
        {
            try
            {
                await SendMessage(messenger.BotId, messenger.ChatId, message, replyMarkup);

                if (logToDatabase)
                {
                    await loggerService.LogMessage(CommonId.BotUserId, messenger.UserId, messenger.ChatId, messenger.BotId, message, MessageSendStatus.Success, functionId);
                }
            }
            catch
            {
                if (logToDatabase)
                {
                    await loggerService.LogMessage(CommonId.BotUserId, messenger.UserId, messenger.ChatId, messenger.BotId, message, MessageSendStatus.Failure, functionId);
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