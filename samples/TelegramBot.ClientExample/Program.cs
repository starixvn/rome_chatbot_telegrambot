using Fanex.Caching;
using Fanex.Caching.Redis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;

namespace TelegramBot.ClientExample
{
    internal static class Program
    {
        private static IBotService _botService;
        private static ISettingService _settingService;
        private static ICommandService _commandService;
        private static ICacheService _cacheService;
        private static Result _result;
        private static BotEntity _firstBot;
        private const int custId = 68287;
        private const int subAccId = 10911;
        private const long chatId = 1293288125;

        private const string system = "IBCBET";

        private static async Task Main()
        {
            await Init();
            await TestSetting();

            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task Init()
        {
            var configurationOptions = ConfigurationOptions.Parse(ConfigurationManager.AppSettings["Fanex.Caching.Redis"]);
            var redisCacheOptions = new RedisCacheOptions
            {
                ApplicationName = configurationOptions.ClientName,
                ConnectionOptions = configurationOptions
            };
            CacheServiceManager.Use(() => new RedisCache(redisCacheOptions));
            _cacheService = new CacheService();
            var httpClient = CreateHttpClient();
            _botService = new BotService(httpClient);
            _settingService = new SettingService(httpClient);
            _commandService = new CommandService(httpClient);

            var bots = await _botService.GetAllAsync();
            Write("Get all bots", bots);

            _firstBot = bots.First();
        }

        private static async Task TestSetting()
        {
            var startUri = await _settingService.GenerateStartUriAsync(custId, subAccId, system);
            Write("Generate start uri", startUri);

            var telegramCommand = $"/start {startUri.Substring(startUri.IndexOf("=", StringComparison.InvariantCulture) + 1)}";
            _result = await _settingService.StartAsync(_firstBot.MessengerBotId, chatId, telegramCommand, "en-US");
            Write("Start and save chat", _result);

            var cacheKey = $"{custId}_{subAccId}";
            var otp = await _cacheService.GetAsync<string>(cacheKey);
            Write("Current OTP", otp);

            _result = await _settingService.LinkAsync(_firstBot.BotId, custId, subAccId, $"${otp}aa");
            Write("Link with incorrect OTP", _result);

            _result = await _settingService.ResendOtpAsync(custId, subAccId);
            Write("Resend new OTP", _result);

            otp = await _cacheService.GetAsync<string>(cacheKey);
            Write("New OTP", otp);

            _result = await _settingService.LinkAsync(_firstBot.BotId, custId, subAccId, otp);
            Write("Link with correct OTP", _result);

            var custBotMessenger = await _settingService.GetCustBotMessengerAsync(custId, subAccId);
            Write("CustBotMessenger", custBotMessenger);

            _result = await _settingService.UnlinkAsync(_firstBot.BotId, custId, subAccId);
            Write("Unlink", _result);
        }

        private static void Write(string header, object value)
        {
            Console.WriteLine($"--- {header} ---");
            Console.WriteLine(JsonConvert.SerializeObject(value));
            Console.WriteLine();
        }

        private static HttpClient CreateHttpClient()
        {
            var uri = ConfigurationManager.AppSettings["TelegramBotWebApiUri"];
            var client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-TW");
            return client;
        }
    }
}