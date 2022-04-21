using Fanex.Caching;
using Fanex.Data.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBot.Common.Extensions;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Setting
{
    public interface ISettingService
    {
        Task<CustBotMessenger> GetCustBotMessenger(int custId, int subAccId);

        Task<Result> Start(long messengerBotId, long chatId, int custId, int subAccId, string language, string ip);

        Task<Result> ResendOtp(int botId, int custId, int subAccId);

        Task<Result> Link(int botId, int custId, int subAccId, string otp, string ip, bool reset = false);

        Task<Result> Unlink(int botId, int custId, int subAccId, string ip);

        Task<Result> UpdateFunction(int userid, Function function, int creator, string ip);

        Task<Result> UpdateLanguage(int userId, string language, string ip);

        Task<CustBotAccount> GetCustBotAccount(long chatId, int botId);
    }

    public class SettingService : ISettingService
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IBotService _botService;
        private readonly ICacheService _cacheService;
        private readonly ISenderService _senderService;
        private readonly IBuilderService _builderService;
        private readonly IActionService _actionService;

        public SettingService(
            WebAPIConfiguration webAPIConfiguration,
            IDynamicRepository dynamicRepository,
            IBotService botService,
            ICacheService cacheService,
            ISenderService senderService,
            IBuilderService builderService,
            IActionService actionService)
        {
            _webAPIConfiguration = webAPIConfiguration;
            _dynamicRepository = dynamicRepository;
            _botService = botService;
            _cacheService = cacheService;
            _senderService = senderService;
            _builderService = builderService;
            _actionService = actionService;
        }

        public async Task<Result> Start(long messengerBotId, long chatId, int custId, int subAccId, string language, string ip)
        {
            var bot = await _botService.GetAsync(messengerBotId);
            var criteria = new CustBotAccountInsCriteria(bot.BotId, chatId, custId, subAccId, language, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
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
            var message = _builderService.BuildOTPMessage(language, otp);
            await _senderService.SendMessage(messengerBotId, chatId, message, ParseMode.Html);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> ResendOtp(int botId, int custId, int subAccId)
        {
            var bot = await _botService.GetAsync(botId);
            var otp = GenerateOtp();
            var cacheKey = $"otp_value_{custId}_{subAccId}";
            var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_webAPIConfiguration.OtpExpirationInMinutes));
            await _cacheService.SetAsync(cacheKey, otp, cacheItemOptions);
            var custBotMessenger = await GetCustBotMessenger(custId, subAccId);
            var message = _builderService.BuildOTPMessage(custBotMessenger.Language, otp);
            await _senderService.SendMessage(bot.MessengerBotId, custBotMessenger.ChatId, message, ParseMode.Html);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> Link(int botId, int custId, int subAccId, string otp, string ip, bool reset = false)
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
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            if (criteria.IsSuccess)
            {
                var bot = await _botService.GetAsync(botId);
                var custBotMessenger = await GetCustBotMessenger(custId, subAccId);


                await _senderService.SendMessage(
                    bot.MessengerBotId,
                    custBotMessenger.ChatId,
                    _builderService.BuildWelcomeMessage(custBotMessenger.Language, custBotMessenger.ChatId),
                    replyMarkup: _actionService.GetRootActions());
            }

            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<Result> Unlink(int botId, int custId, int subAccId, string ip)
        {
            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, false, false, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<CustBotMessenger> GetCustBotMessenger(int custId, int subAccId)
        {
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, !_webAPIConfiguration.IsProduction);
            var resultSet = await _dynamicRepository.FetchMultipleAsync<CustBotMessenger, Function>(criteria).ConfigureAwait(false);

            var custBotMessenger = resultSet.firstResultSet.FirstOrDefault();
            if (custBotMessenger == null)
            {
                return null;
            }

            custBotMessenger.Functions = resultSet.secondResultSet;
            return custBotMessenger;
        }

        public async Task<Result> UpdateFunction(int userid, Function function, int creator, string ip)
        {
            var criteria = new CustBotFunctionUpdCriteria(userid, function, creator, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        private static string GenerateOtp()
        {
            const int maxValue = 999999;
            return ThreadSafeRandom.Next(0, maxValue).ToString("D6");
        }

        public async Task<Result> UpdateLanguage(int userId, string language, string ip)
        {
            var command = new CustBotAccountLanguageUpdCriteria(userId, language, ip);
            await _dynamicRepository.ExecuteAsync(command).ConfigureAwait(false);
            return new Result(command.ErrCode, command.ErrMsg);
        }

        public async Task<CustBotAccount> GetCustBotAccount(long chatId, int botId)
        {
            var criteria = new CustBotUserSelCriteria(chatId, botId);
            return await _dynamicRepository.GetAsync<CustBotAccount>(criteria);
        }
    }
}