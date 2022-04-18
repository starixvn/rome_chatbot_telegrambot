using Fanex.Caching;
using Fanex.Data.Repository;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Common.Extensions;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Setting
{
    public interface ISettingService
    {
        Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId);

        Task<Result> StartAsync(long messengerBotId, long chatId, int custId, int subAccId, string language, string ip);

        Task<Result> ResendOtpAsync(int botId, int custId, int subAccId);

        Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string ip, bool reset = false);

        Task<Result> UnlinkAsync(int botId, int custId, int subAccId, string ip);

        Task<Result> UpdateFunctionsAsync(int custId, int subAccId, string setting, int creator, string ip);
    }

    public class SettingService : ISettingService
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IBotService _botService;
        private readonly ICacheService _cacheService;
        private readonly ISenderService _senderService;
        private readonly IBuilderService _builderService;

        public SettingService(
            WebAPIConfiguration webAPIConfiguration,
            IDynamicRepository dynamicRepository,
            IBotService botService,
            ICacheService cacheService,
            ISenderService senderService,
            IBuilderService builderService)
        {
            _webAPIConfiguration = webAPIConfiguration;
            _dynamicRepository = dynamicRepository;
            _botService = botService;
            _cacheService = cacheService;
            _senderService = senderService;
            _builderService = builderService;
        }

        public async Task<Result> StartAsync(long messengerBotId, long chatId, int custId, int subAccId, string language, string ip)
        {
            var bot = await _botService.GetAsync(messengerBotId);
            var criteria = new CustBotAccountInsCriteria(bot.BotId, chatId, custId, subAccId, language, ip);
            await _dynamicRepository.ExecuteAsync(criteria);
            if (!criteria.IsSuccess)
            {
                if (criteria.ErrCode == ErrorCodes.AccountAlreadyLinked)
                {
                    var errorMsg = _builderService.BuildAccountAlreadyLinkedMessage(language);
                    await _senderService.SendMessage(messengerBotId, chatId, errorMsg, ParseMode.Html);
                }

                if (criteria.ErrCode == ErrorCodes.TelegramAccountAlreadyLinked)
                {
                    var errorMsg = _builderService.BuildTelegramAccountAlreadyLinkedMessage(language);
                    await _senderService.SendMessage(messengerBotId, chatId, errorMsg, ParseMode.Html);
                }

                return new Result(criteria.ErrCode, criteria.ErrMsg);
            }

            var otp = GenerateOtp();
            var cacheKey = $"otp_value_{custId}_{subAccId}";
            var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_webAPIConfiguration.OtpExpirationInMinutes));
            await _cacheService.SetAsync(cacheKey, otp, cacheItemOptions);
            var message = string.Format(ResourceHelper.Get(nameof(SharedResource.Setting_OTP_Help)), otp);
            await _senderService.SendMessage(messengerBotId, chatId, message, ParseMode.Html);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> ResendOtpAsync(int botId, int custId, int subAccId)
        {
            var bot = await _botService.GetAsync(botId);
            var otp = GenerateOtp();
            var cacheKey = $"otp_value_{custId}_{subAccId}";
            var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_webAPIConfiguration.OtpExpirationInMinutes));
            await _cacheService.SetAsync(cacheKey, otp, cacheItemOptions);
            var message = string.Format(ResourceHelper.Get(nameof(SharedResource.Setting_OTP_Help)), otp);
            var custBotMessenger = await GetCustBotMessengerAsync(custId, subAccId);
            await _senderService.SendMessage(bot.MessengerBotId, custBotMessenger.ChatId, message, ParseMode.Html);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp, string ip, bool reset = false)
        {
            var countCacheKey = $"otp_count_{custId}_{subAccId}";
            var failedCount = 0;
            if (reset)
            {
                await _cacheService.RemoveAsync(countCacheKey);
            }
            else
            {
                failedCount = await _cacheService.GetAsync<int>(countCacheKey);
            }

            if (failedCount >= _webAPIConfiguration.OtpMaxFailedTimes)
            {
                return new Result(ErrorCodes.OTP_FailedManyTimes, "OTP failed many times");
            }

            var valueCacheKey = $"otp_value_{custId}_{subAccId}";
            var expectedOtp = await _cacheService.GetAsync<string>(valueCacheKey);
            var isFailed = string.IsNullOrEmpty(expectedOtp) || otp != expectedOtp;
            if (isFailed)
            {
                failedCount++;
                await _cacheService.SetAsync(countCacheKey, failedCount);
                if (failedCount >= _webAPIConfiguration.OtpMaxFailedTimes)
                {
                    return new Result(ErrorCodes.OTP_FailedManyTimes, "OTP failed many times");
                }
            }

            if (string.IsNullOrEmpty(expectedOtp))
            {
                return new Result(ErrorCodes.OTP_Expired, "OTP is expired");
            }

            if (otp != expectedOtp)
            {
                return new Result(ErrorCodes.OTP_Invalid, "OTP is invalid");
            }

            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, true, true, ip);
            await _dynamicRepository.ExecuteAsync(criteria);
            if (criteria.IsSuccess)
            {
                var bot = await _botService.GetAsync(botId);
                var custBotMessenger = await GetCustBotMessengerAsync(custId, subAccId);
                InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Outstanding", "Outstanding"),
                        InlineKeyboardButton.WithCallbackData("Balance Info", "BalanceInfo"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Today Win Loss", "TodayWinLoss"),
                        InlineKeyboardButton.WithCallbackData("Yesterday Win Loss", "YesterdayWinLoss"),
                    },
                     new []
                    {
                        InlineKeyboardButton.WithCallbackData("Settings", "Settings"),
                    },
                });

                await _senderService.SendMessage(
                    bot.MessengerBotId,
                    custBotMessenger.ChatId,
                    string.Format(ResourceHelper.Get(nameof(SharedResource.WelcomeMessage)), custBotMessenger.ChatId),
                    replyMarkup: inlineKeyboard);
            }

            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<Result> UnlinkAsync(int botId, int custId, int subAccId, string ip)
        {
            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, false, false, ip);
            await _dynamicRepository.ExecuteAsync(criteria);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<CustBotMessenger> GetCustBotMessengerAsync(int custId, int subAccId)
        {
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, !_webAPIConfiguration.IsProduction);
            return await _dynamicRepository.GetAsync<CustBotMessenger>(criteria);
        }

        public async Task<Result> UpdateFunctionsAsync(int custId, int subAccId, string setting, int creator, string ip)
        {
            var criteria = new CustBotFunctionUpdCriteria(custId, subAccId, setting, creator, ip);
            await _dynamicRepository.ExecuteAsync(criteria);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        private static string GenerateOtp()
        {
            const int maxValue = 999999;
            return ThreadSafeRandom.Next(0, maxValue).ToString("D6");
        }
    }
}