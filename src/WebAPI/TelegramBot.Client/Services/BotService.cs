using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Common.Models;

namespace TelegramBot.Client.Services
{
    public interface IBotService
    {
        Task<IEnumerable<BotEntity>> GetAllAsync();
    }

    public class BotService : IBotService
    {
        private readonly HttpClient httpClient;

        public BotService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<BotEntity>> GetAllAsync()
        {
            return await httpClient.PostAsJsonAsync<IEnumerable<BotEntity>>($"bot/all");
        }
    }
}