using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;
using TelegramBot.Constants;

namespace TelegramBot.Client.Services
{
    public interface ISettingService
    {
        Task<string> GenerateStartUriAsync(int custId, int subAccId, string system, string language);

        Task<Result> StartAsync(long messengerBotId, long chatId, string message, string ip = "");

        Task<Result> ResendOtpAsync(int custId, int subAccId);

        Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string username, string ip = "");

        Task<Result> UnlinkAsync(int custId, int subAccId, string ip = "");

        Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId, string language);

        Task<Result> UpdateUserFunction(int userId, int botId, Function function, int creator, string ip = "");

        Task<Result> UpdateLanguage(int userId, int botId, string language, int creator, string ip = "");

        Task<bool> IsEnableOTP(int custId, int subAccId);
    }

    public class SettingService : ISettingService
    {
        private readonly HttpClient httpClient;

        public SettingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GenerateStartUriAsync(int custId, int subAccId, string system, string language)
            => await httpClient.PostAsStringAsync(
                $"setting/start-uri",
                new
                {
                    custId,
                    subAccId,
                    system,
                    language
                });

        public async Task<Result> StartAsync(long messengerBotId, long chatId, string message, string ip = "")
            => await httpClient.PostAsJsonAsync<Result>(
                $"setting/start",
                new
                {
                    messengerBotId,
                    chatId,
                    message,
                    ip
                });

        public async Task<Result> ResendOtpAsync(int custId, int subAccId)
            => await httpClient.PostAsJsonAsync<Result>(
                $"setting/resend-otp",
                new
                {
                    custId,
                    subAccId
                });

        public async Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string username, string ip = "")
            => await httpClient.PostAsJsonAsync<Result>(
                $"setting/link",
                new
                {
                    botId,
                    custId,
                    subAccId,
                    otp,
                    username,
                    ip
                });

        public async Task<Result> UnlinkAsync(int custId, int subAccId, string ip = "")
            => await httpClient.PostAsJsonAsync<Result>(
                $"setting/unlink",
                new
                {
                    custId,
                    subAccId,
                    ip
                });

        public async Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId, string language)
            => await httpClient.PostAsJsonAsync<CustBotMessenger>(
                $"setting/custBotMessenger",
                new
                {
                    custId,
                    subAccId,
                    language
                });

        public async Task<Result> UpdateUserFunction(int userId, int botId, Function function, int creator, string ip = "")
            => await httpClient.PostAsJsonAsync<Result>(
                $"setting/{userId}/function",
                new
                {
                    function,
                    creator,
                    botId,
                    ip
                });

        public async Task<Result> UpdateLanguage(int userId, int botId, string language, int creator, string ip = "")
            => await httpClient.PostAsJsonAsync<Result>(
                $"setting/{userId}/language",
                new
                {
                    language,
                    botId,
                    creator,
                    ip
                });

        public async Task<bool> IsEnableOTP(int custId, int subAccId)
        {
            var messenger = await GetCustBotMessengerAsync(custId, subAccId, "en-US");
            if (messenger != null && messenger.Functions.FirstOrDefault(f => f.FunctionId == FunctionIds.OTP).Enabled)
            {
                return true;
            }

            return false;
        }
    }
}