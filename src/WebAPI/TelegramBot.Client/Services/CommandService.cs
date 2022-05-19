using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface ICommandService
    {
        Task HandleTextMessage(long messengerBotId, long chatId, string message);
        Task HandleCallbackMessage(long messengerBotId, long chatId, int messageId, string callbackId, string callBackData);
    }

    public class CommandService : ICommandService
    {
        private readonly HttpClient httpClient;

        public CommandService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task HandleTextMessage(long messengerBotId, long chatId, string message)
            => await httpClient.PostAsJsonAsync<Result>($"command/handle/text", new { messengerBotId, chatId, message });

        public async Task HandleCallbackMessage(long messengerBotId, long chatId, int messageId, string callbackId, string callbackData)
            => await httpClient.PostAsJsonAsync<Result>($"command/handle/callback", new { messengerBotId, chatId, messageId, callbackId, callbackData });
    }
}