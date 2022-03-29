using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Common.Models;

namespace TelegramBot.Client.Services
{
    public interface ISettingService
    {
        Task<string> GenerateStartUriAsync(int custId, int subAccId, string system, bool isTest);
        Task<Result> StartAsync(long messengerBotId, long chatId, string message, string messengerName, string language);
        Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp);
        Task<Result> UnlinkAsync(int botId, int custId, int subAccId);
    }

    public class SettingService : ISettingService
    {
        private readonly HttpClient httpClient;

        public SettingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GenerateStartUriAsync(int custId, int subAccId, string system, bool isTest)
        {
            return await httpClient.PostAsStringAsync(
                $"setting/start-uri",
                new
                {
                    custId,
                    subAccId,
                    system,
                    isTest
                });
        }

        public async Task<Result> StartAsync(long messengerBotId, long chatId, string message, string messengerName, string language)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/start",
                new
                {
                    messengerBotId,
                    chatId,
                    message,
                    messengerName,
                    language
                });
        }

        public async Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/link",
                new
                {
                    botId,
                    custId,
                    subAccId,
                    otp
                });
        }

        public async Task<Result> UnlinkAsync(int botId, int custId, int subAccId)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/unlink",
                new
                {
                    botId,
                    custId,
                    subAccId
                });
        }
    }
}
