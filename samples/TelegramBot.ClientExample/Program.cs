using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Services;

namespace TelegramBot.ClientExample
{
    static class Program
    {
        static async Task Main()
        {
            var httpClient = CreateHttpClient();
            var botService = new BotService(httpClient);
            Console.WriteLine(JsonConvert.SerializeObject(await botService.GetAllAsync()));
            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        private static HttpClient CreateHttpClient()
        {
            var telegramApiUri = ConfigurationManager.AppSettings["TelegramBotClientUri"];
            return new HttpClient
            {
                BaseAddress = new Uri(telegramApiUri)
            };
        }
    }
}
