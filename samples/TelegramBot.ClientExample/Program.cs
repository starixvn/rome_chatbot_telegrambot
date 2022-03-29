using Fanex.Caching;
using Fanex.Caching.Redis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Services;
using TelegramBot.Common.Constants;
using TelegramBot.Common.Models;

namespace TelegramBot.ClientExample
{
    static class Program
    {
        static async Task Main()
        {
            var configurationOptions = ConfigurationOptions.Parse(ConfigurationManager.AppSettings["Fanex.Caching.Redis"]);
            var redisCacheOptions = new RedisCacheOptions
            {
                ApplicationName = configurationOptions.ClientName,
                ConnectionOptions = configurationOptions
            };
            CacheServiceManager.Use(() => new RedisCache(redisCacheOptions));
            var cacheService = new CacheService();

            var httpClient = CreateHttpClient();
            IBotService botService = new BotService(httpClient);
            ISettingService settingService = new SettingService(httpClient);
            ICommandService commandService = new CommandService(httpClient);

            var bots = await botService.GetAllAsync();
            Write("Get all bots", bots);

            const int custId = 68287;
            const int subAccId = 10911;
            const long chatId = 1293288125;
            Result result;
            var firstBot = bots.First();

            var startUri = await settingService.GenerateStartUriAsync(custId, subAccId, "IBC", false);
            Write("Generate start uri", startUri);

            var telegramCommand = $"/start {startUri.Substring(startUri.IndexOf("=", StringComparison.InvariantCulture) + 1)}";
            result = await settingService.StartAsync(firstBot.MessengerBotId, chatId, telegramCommand, "test", LanguageCodes.English);
            Write("Start and save chat", result);

            var cacheKey = $"{custId}_{subAccId}";
            var otp = await cacheService.GetAsync<string>(cacheKey);
            Write("Current OTP", otp);

            result = await settingService.LinkAsync(firstBot.BotId, custId, subAccId, $"${otp}aa");
            Write("Link with incorrect OTP", result);

            result = await settingService.LinkAsync(firstBot.BotId, custId, subAccId, otp);
            Write("Link with correct OTP", result);

            result = await settingService.UnlinkAsync(firstBot.BotId, custId, subAccId);
            Write("Unlink", result);

            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        private static void Write(string header, object value)
        {
            Console.WriteLine($"--- {header} ---");
            Console.WriteLine(JsonConvert.SerializeObject(value));
            Console.WriteLine();
        }

        private static HttpClient CreateHttpClient()
        {
            var telegramApiUri = ConfigurationManager.AppSettings["TelegramBotClientUri"];
            var client = new HttpClient
            {
                BaseAddress = new Uri(telegramApiUri)
            };
            client.DefaultRequestHeaders.Add("Accept-Language", LanguageCodes.Taiwan);
            return client;
        }
    }
}
