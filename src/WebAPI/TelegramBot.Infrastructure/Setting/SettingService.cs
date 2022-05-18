using Alpha.API.Client.Services;
using Fanex.Caching;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Common.Extensions;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Setting
{
    public interface ISettingService
    {
        Task<CustBotMessenger> GetCustBotMessenger(int custId, int subAccId);

        Task<Result> Start(int botId, long chatId, int custId, int subAccId, string language, string ip);

        Task<Result> ResendOtp(int custId, int subAccId);

        Task<Result> Link(int botId, int custId, int subAccId, string otp, string ip);

        Task<Result> Unlink(int botId, int custId, int subAccId, string ip);

        Task<Result> UpdateUserFunction(int userid, BotFunction function, int creator, string ip);

        Task<Result> UpdateUserLanguage(int userId, string language, string ip);

        Task<CustBotAccount> GetCustBotAccount(long chatId, int botId);
    }

    public class SettingService : ISettingService
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly ICacheService _cacheService;
        private readonly IMessageSender _messageSender;
        private readonly IMessageBuilder _messageBuilder;
        private readonly IActionService _actionService;
        private readonly ICustomerService _customerService;
        private readonly IFunctionService _functionService;

        public SettingService(
            WebAPIConfiguration webAPIConfiguration,
            IDynamicRepository dynamicRepository,
            ICacheServiceFactory cacheServiceFactory,
            IMessageSender messageSender,
            IMessageBuilder messageBuilder,
            IActionService actionService,
            ICustomerService customerService,
            IFunctionService functionService)
        {
            _webAPIConfiguration = webAPIConfiguration;
            _dynamicRepository = dynamicRepository;
            _cacheService = cacheServiceFactory.Get(CacheProvider.Redis);
            _messageSender = messageSender;
            _messageBuilder = messageBuilder;
            _actionService = actionService;
            _customerService = customerService;
            _functionService = functionService;
        }

        public async Task<Result> Start(int botId, long chatId, int custId, int subAccId, string language, string ip)
        {
            var criteria = new CustBotAccountInsCriteria(botId, chatId, custId, subAccId, language, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            var messenger = new Messenger(botId, chatId);
            if (!criteria.IsSuccess)
            {
                if (criteria.ErrCode == ErrorCodes.AccountAlreadyLinked)
                {
                    var errorMsg = _messageBuilder.BuildAccountAlreadyLinkedMessage(language);
                    await _messageSender.SendMessage(criteria.UserId, messenger, errorMsg);
                }

                if (criteria.ErrCode == ErrorCodes.TelegramAccountAlreadyLinked)
                {
                    var errorMsg = _messageBuilder.BuildTelegramAccountAlreadyLinkedMessage(language);
                    await _messageSender.SendMessage(criteria.UserId, messenger, errorMsg);
                }

                return new Result(criteria.ErrCode, criteria.ErrMsg);
            }

            var otp = GenerateOtp();
            var cacheKey = $"otp_value_{custId}_{subAccId}";
            var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_webAPIConfiguration.OtpExpirationInMinutes));
            await _cacheService.SetAsync(cacheKey, otp, cacheItemOptions);
            var message = _messageBuilder.BuildOTPMessage(language, otp);
            await _messageSender.SendMessage(messenger, message);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> ResendOtp(int custId, int subAccId)
        {
            var otp = GenerateOtp();
            var cacheKey = $"otp_value_{custId}_{subAccId}";
            var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_webAPIConfiguration.OtpExpirationInMinutes));
            await _cacheService.SetAsync(cacheKey, otp, cacheItemOptions);
            var custBotMessenger = await GetVerifyingMessenger(custId, subAccId);
            var message = _messageBuilder.BuildOTPMessage(custBotMessenger.Language, otp);
            await _messageSender.SendMessage(custBotMessenger.UserId, new Messenger(custBotMessenger.BotId, custBotMessenger.ChatId), message);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> Link(int botId, int custId, int subAccId, string otp, string ip)
        {
            var countCacheKey = $"otp_count_{custId}_{subAccId}";
            var failedCount = await _cacheService.GetAsync<int>(countCacheKey);
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
                await _cacheService.RemoveAsync(countCacheKey);
                await SendWelcomeMessage(custId, subAccId);
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
            var resultSet = await _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(criteria).ConfigureAwait(false);

            var custBotMessenger = resultSet.firstResultSet.FirstOrDefault();
            if (custBotMessenger == null)
            {
                return null;
            }

            custBotMessenger.Functions = await GetAccountFunctions(custId, subAccId, resultSet.secondResultSet);
            foreach (var function in custBotMessenger.Functions)
            {
                function.FunctionName = await _functionService.GetFunctionName(function.FunctionId, custBotMessenger.Language);
                function.Description = await _functionService.GetFunctionDescription(function.FunctionId, custBotMessenger.Language);
            }

            return custBotMessenger;
        }

        private async Task<IEnumerable<BotFunction>> GetAccountFunctions(int custId, int subAccId, IEnumerable<BotFunction> functions)
        {
            if (subAccId > 0)
            {
                var customerStatus = await _customerService.GetCustomerStatus(custId, subAccId);
                if (customerStatus.HasReportPermission)
                {
                    functions = functions.Concat(await _functionService.GetReportFunctions());
                }

                functions = functions.Where(f => !customerStatus.HasReportPermission && f.FunctionId != FunctionIds.YesterdayWinLossNotification);
                functions = functions.Where(f => !customerStatus.HasMemberInformationPermission && f.FunctionId != FunctionIds.ProblemAccount && f.FunctionId != FunctionIds.MemberWinLossLimit);
            }
            else
            {
                functions = functions.Concat(await _functionService.GetReportFunctions());
            }

            return functions.Concat(await _functionService.GetCommonFunctions());
        }

        private async Task<CustBotMessenger> GetVerifyingMessenger(int custId, int subAccId)
        {
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, !_webAPIConfiguration.IsProduction, true, false);
            var resultSet = await _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(criteria).ConfigureAwait(false);
            return resultSet.firstResultSet.FirstOrDefault();
        }

        public async Task<Result> UpdateUserFunction(int userid, BotFunction function, int creator, string ip)
        {
            var criteria = new CustBotFunctionUpdCriteria(userid, function, creator, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<Result> UpdateUserLanguage(int userId, string language, string ip)
        {
            var command = new CustBotAccountLanguageUpdCriteria(userId, language, ip);
            await _dynamicRepository.ExecuteAsync(command).ConfigureAwait(false);
            return new Result(command.ErrCode, command.ErrMsg);
        }

        public async Task<CustBotAccount> GetCustBotAccount(long chatId, int botId)
        {
            var criteria = new CustBotUserSelCriteria(chatId, botId);
            var resultSet = await _dynamicRepository.FetchMultipleAsync<CustBotAccount, BotFunction>(criteria);
            var account = resultSet.firstResultSet.FirstOrDefault();
            account.Functions = await GetAccountFunctions(account.CustId, account.SubAccId, resultSet.secondResultSet);
            return account;
        }

        private static string GenerateOtp()
        {
            const int maxValue = 999999;
            return ThreadSafeRandom.Next(0, maxValue).ToString("D6");
        }

        private async Task SendWelcomeMessage(int custId, int subAccId)
        {
            var messenger = await GetCustBotMessenger(custId, subAccId);
            var account = await GetCustBotAccount(messenger.ChatId, messenger.BotId);
            var text = _messageBuilder.BuildWelcomeMessage(messenger.Language, messenger.ChatId);
            var rootActionButtons = await _actionService.BuildRootActionButtons(account);
            var message = await _messageBuilder.BuildMessage(text, rootActionButtons, messenger.Language);
            await _messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);
        }
    }
}