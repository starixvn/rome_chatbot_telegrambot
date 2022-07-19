using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId);

        Task<Result> Unlink(int userId, long chatId, int botId, string language);
    }

    public class NotificationService : INotificationService
    {
        private readonly HttpClient httpClient;

        public NotificationService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Result> NotifyChangePassword(int custId, int subAccId)
            => await httpClient.PostAsJsonAsync<Result>(
                $"notification/change-password-alert",
                new
                {
                    custId,
                    subAccId
                });

        public async Task<Result> NotifyChangeSecurityCode(int custId, int subAccId)
             => await httpClient.PostAsJsonAsync<Result>(
                $"notification/change-security-code-alert",
                new
                {
                    custId,
                    subAccId
                });

        public async Task<Result> Unlink(int userId, long chatId, int botId, string language)
             => await httpClient.PostAsJsonAsync<Result>(
                $"notification/{userId}/unlink",
                new
                {
                    chatId,
                    botId,
                    language
                });
    }
}