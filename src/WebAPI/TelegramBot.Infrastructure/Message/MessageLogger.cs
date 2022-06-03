using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Message.Criteria;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageLogger
    {
        Task LogMessage(int sender, int receiver, Messenger messenger, Models.Message message, byte status = 0, short functionId = 0);

        Task LogMessage(int sender, int receiver, Messenger messenger, string message, byte status = 0, short functionId = 0);
    }

    public class MessageLogger : IMessageLogger
    {
        private readonly IDynamicRepository dynamicRepository;

        public MessageLogger(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task LogMessage(int sender, int receiver, Messenger messenger, string message, byte status = 0, short functionId = 0)
        {
            var command = new CustBotMessageInsCriteria(sender, receiver, messenger.ChatId, messenger.BotId, functionId, message, status);
            await dynamicRepository.ExecuteAsync(command).ConfigureAwait(false);
        }

        public async Task LogMessage(int sender, int receiver, Messenger messenger, Models.Message message, byte status = 0, short functionId = 0)
        {
            var logMessage = new List<string> { message.Text };

            if (message.ReplyMarkup != null)
            {
                foreach (var inlineKeyboard in message.ReplyMarkup.InlineKeyboard)
                {
                    foreach (var button in inlineKeyboard)
                    {
                        logMessage.Add($"<a href='#'>{button.Text}</a>");
                    }
                }
            }

            await LogMessage(
              sender,
              receiver,
              messenger,
              string.Join("<br>", logMessage),
              status,
              functionId);
        }
    }
}