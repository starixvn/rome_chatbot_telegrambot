using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;

namespace TelegramBot.Client.Services
{
    public interface IOTPService
    {
        Task SendOTP(int custId, int subAccId);
        Task<Result> VerifyOTP(int custId, int subAccId, string otp);
    }

    public class OTPService : IOTPService
    {
        private readonly HttpClient httpClient;

        public OTPService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task SendOTP(int custId, int subAccId)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { custId, subAccId }), Encoding.UTF8, "application/json");
            using var responseMessage = await httpClient.PostAsync("OTP/Send", content).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task<Result> VerifyOTP(int custId, int subAccId, string otp)
        {
            return await httpClient.PostAsJsonAsync<Result>("OTP/Verify", new { custId, subAccId, otp });
        }
    }
}