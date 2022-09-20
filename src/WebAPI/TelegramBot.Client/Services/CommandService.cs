using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface ICommandService
    {
        Task<Result> HandleTextMessage(long messengerBotId, long chatId, string message);
        Task<Result> HandleCallbackMessage(long messengerBotId, long chatId, int messageId, string callbackId, string message);
    }

    public class CommandService : ICommandService
    {
        private readonly HttpClient httpClient;

        public CommandService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Result> HandleTextMessage(long messengerBotId, long chatId, string message)
            => await httpClient.PostAsJsonAsync<Result>($"command/handle/text", new { messengerBotId, chatId, message });

        public async Task<Result> HandleCallbackMessage(long messengerBotId, long chatId, int messageId, string callbackId, string message)
            => await httpClient.PostAsJsonAsync<Result>($"command/handle/callback", new { messengerBotId, chatId, messageId, callbackId, message });
    }
}