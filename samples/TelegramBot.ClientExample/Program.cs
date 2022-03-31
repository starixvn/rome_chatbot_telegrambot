using Fanex.Caching;
using Fanex.Caching.Redis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Constants;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using TelegramBot.Common.Services;

namespace TelegramBot.ClientExample
{
    static class Program
    {
        private static IBotService botService;
        private static ISettingService settingService;
        private static ICommandService commandService;
        private static ICacheService cacheService;
        const int custId = 68287;
        const int subAccId = 10911;
        const long chatId = 1293288125;
        const bool isTest = false;
        const string system = "IBC";
        const string messengerName = "test";

        static async Task Main()
        {
            Init();

            await TestLogging();
            await TestSetting();

            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task TestLogging()
        {
            try
            {
                const string plainKey = "a:2";
                var cryptography = new Cryptography();
                var encryptKey = cryptography.Encrypt(plainKey);
                var message = $"start {encryptKey}";
                var result = await settingService.StartAsync(1, 1, message, "test", LanguageCodes.English);
                Write("Test logging", result);
            }
            catch (Exception ex)
            {
                Write("Test logging", ex);
            }
        }

        private static void Init()
        {
            var configurationOptions = ConfigurationOptions.Parse(ConfigurationManager.AppSettings["Fanex.Caching.Redis"]);
            var redisCacheOptions = new RedisCacheOptions
            {
                ApplicationName = configurationOptions.ClientName,
                ConnectionOptions = configurationOptions
            };
            CacheServiceManager.Use(() => new RedisCache(redisCacheOptions));
            cacheService = new CacheService();
            var httpClient = CreateHttpClient();
            botService = new BotService(httpClient);
            settingService = new SettingService(httpClient);
            commandService = new CommandService(httpClient);
        }

        private static async Task TestSetting()
        {
            var bots = await botService.GetAllAsync();
            Write("Get all bots", bots);

            Result result;
            var firstBot = bots.First();

            var startUri = await settingService.GenerateStartUriAsync(custId, subAccId, system, isTest);
            Write("Generate start uri", startUri);

            var telegramCommand = $"/start {startUri.Substring(startUri.IndexOf("=", StringComparison.InvariantCulture) + 1)}";
            result = await settingService.StartAsync(firstBot.MessengerBotId, chatId, telegramCommand, messengerName, LanguageCodes.English);
            Write("Start and save chat", result);

            var cacheKey = $"{custId}_{subAccId}";
            var otp = await cacheService.GetAsync<string>(cacheKey);
            Write("Current OTP", otp);

            result = await settingService.LinkAsync(firstBot.BotId, custId, subAccId, $"${otp}aa");
            Write("Link with incorrect OTP", result);

            result = await settingService.ResendOtpAsync(firstBot.BotId, custId, subAccId, isTest);
            Write("Resend new OTP", result);

            otp = await cacheService.GetAsync<string>(cacheKey);
            Write("New OTP", otp);

            result = await settingService.LinkAsync(firstBot.BotId, custId, subAccId, otp);
            Write("Link with correct OTP", result);

            var custBotMessenger = await settingService.GetCustBotMessengerAsync(custId, subAccId, isTest);
            Write("CustBotMessenger", custBotMessenger);

            result = await settingService.UnlinkAsync(firstBot.BotId, custId, subAccId);
            Write("Unlink", result);
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
