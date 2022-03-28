using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Services;
using TelegramBot.Common.Constants;

namespace TelegramBot.ClientExample
{
    static class Program
    {
        static async Task Main()
        {
            var httpClient = CreateHttpClient();
            var botService = new BotService(httpClient);
            var settingService = new SettingService(httpClient);
            var commandService = new CommandService(httpClient);

            var bots = await botService.GetAllAsync();
            Console.WriteLine(JsonConvert.SerializeObject(bots));

            const int custId = 68287;
            const int subAccId = 10911;
            var startUri = await settingService.GenerateStartUriAsync(custId, subAccId, "IBC", false);
            Console.WriteLine(startUri);

            var firstBot = bots.First();
            var telegramCommand = $"/start {startUri.Substring(startUri.IndexOf("=", StringComparison.InvariantCulture) + 1)}";

            const long chatId = 1293288125;
            var result = await settingService.StartAsync(firstBot.MessengerBotId, chatId, telegramCommand, "test", LanguageCodes.English);
            Console.WriteLine(JsonConvert.SerializeObject(result));

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
