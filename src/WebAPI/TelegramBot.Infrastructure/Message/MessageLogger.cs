using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Message.Criteria;
using TelegramBot.Infrastructure.Message.Models;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageLogger
    {
        Task LogMessage(int sender, int receiver, long chatId, int botId, string message, IList<MessageButton> buttons, byte status = 0, short functionId = 0);

        Task LogMessage(int sender, int receiver, long chatId, int botId, string message, byte status = 0, short functionId = 0);
    }

    public class MessageLogger : IMessageLogger
    {
        private readonly IDynamicRepository dynamicRepository;

        public MessageLogger(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task LogMessage(int sender, int receiver, long chatId, int botId, string message, byte status = 0, short functionId = 0)
        {
            var command = new CustBotMessageInsCriteria(sender, receiver, chatId, botId, functionId, message, status);
            await dynamicRepository.ExecuteAsync(command).ConfigureAwait(false);
        }

        public async Task LogMessage(int sender, int receiver, long chatId, int botId, string message, IList<MessageButton> buttons, byte status = 0, short functionId = 0)
        {
            var logMessage = new List<string>{
                message
            };

            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    logMessage.Add($"<a href='#' data-hidden='{button.CallBackData}'>{button.Text}</a>");
                }
            }

            await LogMessage(
              sender,
              receiver,
              chatId,
              botId,
              string.Join("<br>", logMessage),
              status,
              functionId);
        }
    }
}