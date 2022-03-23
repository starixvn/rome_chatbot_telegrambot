using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;

namespace TelegramBot.Client.Services
{
    public interface ISettingService
    {
        Task<string> GenerateStartUriAsync(int custId, int subAccountId, string system, bool isTest);
    }

    public class SettingService : ISettingService
    {
        private readonly HttpClient httpClient;

        public SettingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GenerateStartUriAsync(int custId, int subAccountId, string system, bool isTest)
        {
            return await httpClient.PostAsStringAsync(
                $"setting/{custId}/{subAccountId}/start-uri",
                new
                {
                    system,
                    isTest
                });
        }
    }
}
