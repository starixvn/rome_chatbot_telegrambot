using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Common.Models;

namespace TelegramBot.Client.Services
{
    public interface ICommandService
    {
        Task<Result> ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback);
    }

    public class CommandService : ICommandService
    {
        private readonly HttpClient httpClient;

        public CommandService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Result> ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"command/execute",
                new
                {
                    messengerBotId,
                    chatId,
                    message,
                    isCallback
                });
        }
    }
}