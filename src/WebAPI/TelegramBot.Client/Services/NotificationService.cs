using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId, string ip);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip);
    }

    public class NotificationService : INotificationService
    {
        private readonly HttpClient httpClient;

        public NotificationService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Result> NotifyChangePassword(int custId, int subAccId, string ip)
            => await httpClient.PostAsJsonAsync<Result>(
                $"notification/change-password-alert",
                new
                {
                    custId,
                    subAccId,
                    ip
                });

        public async Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip)
             => await httpClient.PostAsJsonAsync<Result>(
                $"notification/change-security-code-alert",
                new
                {
                    custId,
                    subAccId,
                    ip
                });
    }
}