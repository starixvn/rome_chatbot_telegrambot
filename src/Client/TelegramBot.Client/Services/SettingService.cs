using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface ISettingService
    {
        Task<string> GenerateStartUriAsync(int custId, int subAccId, string system);
        Task<Result> StartAsync(long messengerBotId, long chatId, string message, string messengerName, string language, string ip = "");
        Task<Result> ResendOtpAsync(int botId, int custId, int subAccId);
        Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string ip = "", bool reset = false);
        Task<Result> UnlinkAsync(int botId, int custId, int subAccId, string ip = "");
        Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId);
    }

    public class SettingService : ISettingService
    {
        private readonly HttpClient httpClient;

        public SettingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GenerateStartUriAsync(int custId, int subAccId, string system)
        {
            return await httpClient.PostAsStringAsync(
                $"setting/start-uri",
                new
                {
                    custId,
                    subAccId,
                    system
                });
        }

        public async Task<Result> StartAsync(long messengerBotId, long chatId, string message, string messengerName, string language, string ip = "")
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/start",
                new
                {
                    messengerBotId,
                    chatId,
                    message,
                    messengerName,
                    language,
                    ip
                });
        }

        public async Task<Result> ResendOtpAsync(int botId, int custId, int subAccId)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/resend-otp",
                new
                {
                    botId,
                    custId,
                    subAccId
                });
        }

        public async Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string ip = "", bool reset = false)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/link",
                new
                {
                    botId,
                    custId,
                    subAccId,
                    otp,
                    ip,
                    reset
                });
        }

        public async Task<Result> UnlinkAsync(int botId, int custId, int subAccId, string ip = "")
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/unlink",
                new
                {
                    botId,
                    custId,
                    subAccId,
                    ip
                });
        }

        public async Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId)
        {
            return await httpClient.PostAsJsonAsync<CustBotMessenger>(
                $"setting/custBotMessenger",
                new
                {
                    custId,
                    subAccId
                });
        }
    }
}
