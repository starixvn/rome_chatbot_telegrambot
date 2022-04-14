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
using TelegramBot.Client.Extensions;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using TelegramBot.Common.Services;

namespace TelegramBot.ClientExample
{
    static class Program
    {
        private static IBotService _botService;
        private static ISettingService _settingService;
        private static ICommandService _commandService;
        private static ICacheService _cacheService;
        private static Result _result;
        private static BotEntity _firstBot;
        const int custId = 68287;
        const int subAccId = 10911;
        const long chatId = 1293288125;

        const string system = "IBCBET";
        const string messengerName = "test";

        static async Task Main()
        {
            await Init();
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
                Write("Test logging Receiver with this telegram start link", $"https://t.me/{_firstBot.Name}?start={encryptKey}");
                _result = await _settingService.StartAsync(1, 1, message, "test", LanguageCodes.English);
                Write("Test logging WebAPI", _result);
            }
            catch (Exception ex)
            {
                Write("Test logging WebAPI", ex);
            }
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
            _result = await _settingService.StartAsync(_firstBot.MessengerBotId, chatId, telegramCommand, messengerName, LanguageCodes.English);
            Write("Start and save chat", _result);

            var cacheKey = $"{custId}_{subAccId}";
            var otp = await _cacheService.GetAsync<string>(cacheKey);
            Write("Current OTP", otp);

            _result = await _settingService.LinkAsync(_firstBot.BotId, custId, subAccId, $"${otp}aa");
            Write("Link with incorrect OTP", _result);

            _result = await _settingService.ResendOtpAsync(_firstBot.BotId, custId, subAccId);
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
            client.DefaultRequestHeaders.Add("Accept-Language", LanguageCodes.Taiwan);
            return client;
        }
    }
}
