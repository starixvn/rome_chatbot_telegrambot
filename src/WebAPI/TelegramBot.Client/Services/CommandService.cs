using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface ICommandService
    {
        Task<Result> ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback, int messageId = 0);
    }

    public class CommandService : ICommandService
    {
        private readonly HttpClient httpClient;

        public CommandService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Result> ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback, int messageId = 0)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"command/execute",
                new
                {
                    messengerBotId,
                    chatId,
                    message,
                    isCallback,
                    messageId
                });
        }
    }
}