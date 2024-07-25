using Fanex.Caching;
using Alpha.API.Client.Services;
using Fanex.Data.Repository;
using TelegramBot.Modules.Action;
using TelegramBot.Modules.Function.Models;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.OTP;
using TelegramBot.Modules.Setting.Constants;
using TelegramBot.Modules.Setting.Criteria;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Configurations;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Models;
using TelegramBot.Shared.Caching;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Modules.AutoTransfer;

namespace TelegramBot.Modules.Setting
{
    public interface ISettingService
    {
        Task<CustBotMessenger> GetCustBotMessenger(int custId, int subAccId, string language = LanguageCodes.English);

        Task<Result> Start(int botId, long chatId, int custId, int subAccId, string language, string ip);

        Task<Result> ResendOtp(int custId, int subAccId);

        Task<Result> Link(int botId, int custId, int subAccId, string otp, string username, string ip);

        Task<Result> Unlink(int custId, int subAccId, string ip);

        Task<Result> UpdateUserFunction(int userid, int botId, BotFunction function, int creator, string ip);

        Task<Result> UpdateUserLanguage(int userId, int botId, string language, int creator, string ip);

        Task<CustBotAccount> GetCustBotAccount(long chatId, int botId);

        Task SendUnlinkMessage(int userId, long chatId, int botId, string botName, string language);

        Task<string> GetSiteDBGroupIp(int custId);
    }

    public class SettingService : ISettingService
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IMessageSender _messageSender;
        private readonly IMessageBuilder _messageBuilder;
        private readonly IActionService _actionService;
        private readonly ICustomerService _customerService;
        private readonly IFunctionService _functionService;
        private readonly IOTPService _otpService;
        private readonly ICacheService cacheService;
        private readonly IAutoTransferService _autoTransferService;

        public SettingService(
            WebAPIConfiguration webAPIConfiguration,
            IDynamicRepository dynamicRepository,
            IMessageSender messageSender,
            IMessageBuilder messageBuilder,
            IActionService actionService,
            ICustomerService customerService,
            IFunctionService functionService,
            IOTPService otpService,
            IAutoTransferService autoTransferService,
            ICacheServiceFactory cacheServiceFactory)
        {
            _webAPIConfiguration = webAPIConfiguration;
            _dynamicRepository = dynamicRepository;
            _messageSender = messageSender;
            _messageBuilder = messageBuilder;
            _actionService = actionService;
            _customerService = customerService;
            _functionService = functionService;
            _otpService = otpService;
            _autoTransferService = autoTransferService;
            cacheService = cacheServiceFactory.Get();
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

            var otp = await _otpService.GenerateOTP(custId, subAccId);
            var message = _messageBuilder.BuildSettingOTPMessage(language, otp);
            await _messageSender.SendMessage(criteria.UserId, messenger, message);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> ResendOtp(int custId, int subAccId)
        {
            var custBotMessenger = await GetVerifyingMessenger(custId, subAccId);
            if (custBotMessenger == null)
            {
                return new Result(ErrorCodes.MessengerNotFound, "Messenger not found");
            }

            var otp = await _otpService.GenerateOTP(custId, subAccId);
            var message = _messageBuilder.BuildSettingOTPMessage(custBotMessenger.Language, otp);
            await _messageSender.SendMessage(custBotMessenger.UserId, new Messenger(custBotMessenger.BotId, custBotMessenger.ChatId), message);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> Link(int botId, int custId, int subAccId, string otp, string username, string ip)
        {
            if ((await GetVerifyingMessenger(custId, subAccId)) == null)
            {
                return new Result(ErrorCodes.MessengerNotFound, "Messenger not found");
            }

            var verifyOTPResult = await _otpService.VerifyOTP(custId, subAccId, otp, _webAPIConfiguration.OtpMaxFailedTimes);
            if (verifyOTPResult.IsNotOk)
            {
                return verifyOTPResult;
            }

            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, true, true, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            if (criteria.IsSuccess)
            {
                await SendWelcomeMessage(custId, subAccId, username);
            }

            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<Result> Unlink(int custId, int subAccId, string ip)
        {
            var messenger = await GetCustBotMessenger(custId, subAccId);
            if (messenger == null)
            {
                return new Result(ErrorCodes.MessengerNotFound, "Messenger not found");
            }

            var criteria = new CustBotAccountStatusUpdCriteria(messenger.BotId, custId, subAccId, false, false, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            if (criteria.IsSuccess)
            {
                await SendUnlinkMessage(messenger.UserId, messenger.ChatId, messenger.BotId, messenger.BotName, messenger.Language);
            }

            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task SendUnlinkMessage(int userId, long chatId, int botId, string botName, string language)
        {
            var unlinkMessage = _messageBuilder.BuildUnlinkMessage(language, botName);
            await _messageSender.SendMessage(userId, new Messenger(botId, chatId), unlinkMessage);
        }

        public async Task<CustBotMessenger> GetCustBotMessenger(int custId, int subAccId, string language = LanguageCodes.English)
        {
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, !_webAPIConfiguration.IsProduction);
            var (custBotMessengers, functions) = await _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(criteria).ConfigureAwait(false);

            var custBotMessenger = custBotMessengers.FirstOrDefault();
            if (custBotMessenger == null)
            {
                return null;
            }

            custBotMessenger.Functions = await GetAccountFunctions(custId, subAccId, functions, custBotMessenger.System, custBotMessenger.CustomerLevel);
            foreach (var function in custBotMessenger.Functions)
            {
                function.FunctionName = await _functionService.GetFunctionName(function.FunctionId, language);
                function.Description = await _functionService.GetFunctionDescription(function.FunctionId, language);
            }

            return custBotMessenger;
        }

        public async Task<string> GetSiteDBGroupIp(int custId)
        {
            var siteDB = await cacheService.GetOrSetAsync(
                $"TelegramBot.Modules.Setting.SiteDBGroup.CustId-{custId}",
                () => _dynamicRepository.GetAsync<SiteDBGroup>(new GetSiteDBConfigCriteria(custId)),
                new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1)));

            return siteDB.AGIP;
        }

        private async Task<IEnumerable<BotFunction>> GetAccountFunctions(
            int custId, 
            int subAccId, 
            IEnumerable<BotFunction> functions, 
            string system,
            byte custLevel)
        {
            functions = functions.Concat(await _functionService.GetReportAndCommonFunctions());

            var isADBT = await _autoTransferService.IsAllDayBalanceTransfer(custId);

            if (subAccId > 0)
            {
                var customerStatus = await _customerService.GetCustomerStatus(custId, subAccId);
                var functionPermissions = _webAPIConfiguration.FunctionPermissions;
                functions = functions.Where(f => IsAllowFunction(f, customerStatus, functionPermissions, isADBT, system, custLevel));
            } else {
                functions = functions.Where(f => IsAllowFunction(f, null, new Dictionary<string,string> { }, isADBT, system, custLevel));
            }

            return functions;
        }

        private bool IsAllowFunction(
            BotFunction f, 
            CustomerStatus customerStatus, 
            IDictionary<string, string> functionPermissions,
            bool isADBT,
            string system,
            byte custLevel) {
            var isAllow = !functionPermissions.ContainsKey(f.FunctionKey)
                || customerStatus.PageAccess.Contains(functionPermissions[f.FunctionKey]);
            switch(f.FunctionKey) {
                case "AutoTransferSettings":
                case "AutoTransferAlert":
                    return isAllow && !isADBT && system != "789Y";
                case "LastBetsAlert":
                    return isAllow && custLevel == (byte)CustomerLevel.Agent;
                default:
                    return isAllow;
            }
        }

        private async Task<CustBotMessenger> GetVerifyingMessenger(int custId, int subAccId)
        {
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, !_webAPIConfiguration.IsProduction, true, false);
            return (await _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(criteria).ConfigureAwait(false)).FirstResultSet.FirstOrDefault();
        }

        public async Task<Result> UpdateUserFunction(int userid, int botId, BotFunction function, int creator, string ip)
        {
            var criteria = new CustBotFunctionUpdCriteria(userid, botId, function, creator, ip);
            await _dynamicRepository.ExecuteAsync(criteria).ConfigureAwait(false);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<Result> UpdateUserLanguage(int userId, int botId, string language, int creator, string ip)
        {
            var command = new CustBotAccountLanguageUpdCriteria(userId, botId, language, creator, ip);
            await _dynamicRepository.ExecuteAsync(command).ConfigureAwait(false);
            return new Result(command.ErrCode, command.ErrMsg);
        }

        public async Task<CustBotAccount> GetCustBotAccount(long chatId, int botId)
        {
            var criteria = new CustBotUserSelCriteria(chatId, botId, !_webAPIConfiguration.IsProduction);
            var (accounts, functions) = await _dynamicRepository.FetchMultipleAsync<CustBotAccount, BotFunction>(criteria);
            var account = accounts.FirstOrDefault();
            if (account != null)
            {
                account.Functions = await GetAccountFunctions(account.CustId, account.SubAccId, functions, account.System, account.CustomerLevel);
                account.SiteDBGroupIp = await GetSiteDBGroupIp(account.CustId);
            }

            return account;
        }

        private async Task SendWelcomeMessage(int custId, int subAccId, string username)
        {
            var messenger = await GetCustBotMessenger(custId, subAccId);
            var text = _messageBuilder.BuildWelcomeMessage(messenger.Language, username);
            var account = await GetCustBotAccount(messenger.ChatId, messenger.BotId);
            var rootActionButtons = await _actionService.BuildRootActionButtons(account);
            var message = _messageBuilder.BuildMessage(text, rootActionButtons, messenger.Language);
            await _messageSender.SendMessage(account, message);
        }
    }
}