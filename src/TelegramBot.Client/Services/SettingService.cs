using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Common.Models;

namespace TelegramBot.Client.Services
{
    public interface ISettingService
    {
        Task<string> GenerateStartUriAsync(int custId, int subAccId, string system, bool isTest);
        Task<Result> StartAsync(long messengerBotId, long chatId, string message, string messengerName, string language, string ip = "");
        Task<Result> ResendOtpAsync(int botId, int custId, int subAccId, bool isTest);
        Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string ip = "");
        Task<Result> UnlinkAsync(int botId, int custId, int subAccId, string ip = "");
        Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId, bool isTest);
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

        public async Task<Result> ResendOtpAsync(int botId, int custId, int subAccId, bool isTest)
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/resend-otp",
                new
                {
                    botId,
                    custId,
                    subAccId,
                    isTest
                });
        }

        public async Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string ip = "")
        {
            return await httpClient.PostAsJsonAsync<Result>(
                $"setting/link",
                new
                {
                    botId,
                    custId,
                    subAccId,
                    otp,
                    ip
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

        public async Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId, bool isTest)
        {
            return await httpClient.PostAsJsonAsync<CustBotMessenger>(
                $"setting/custBotMessenger",
                new
                {
                    custId,
                    subAccId,
                    isTest
                });
        }
    }
}
