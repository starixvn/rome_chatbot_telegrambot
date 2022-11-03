using Alpha.API.Client.Services;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.OTP;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Setting
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
        private readonly IOTPService _otpservice;

        public SettingService(
            WebAPIConfiguration webAPIConfiguration,
            IDynamicRepository dynamicRepository,
            IMessageSender messageSender,
            IMessageBuilder messageBuilder,
            IActionService actionService,
            ICustomerService customerService,
            IFunctionService functionService,
            IOTPService otpservice)
        {
            _webAPIConfiguration = webAPIConfiguration;
            _dynamicRepository = dynamicRepository;
            _messageSender = messageSender;
            _messageBuilder = messageBuilder;
            _actionService = actionService;
            _customerService = customerService;
            _functionService = functionService;
            _otpservice = otpservice;
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

            var otp = await _otpservice.GenerateOTP(custId, subAccId);
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

            var otp = await _otpservice.GenerateOTP(custId, subAccId);
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

            var verifyOTPResult = await _otpservice.VerifyOTP(custId, subAccId, otp);
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

            custBotMessenger.Functions = await GetAccountFunctions(custId, subAccId, custBotMessenger.CustomerLevel, functions);
            foreach (var function in custBotMessenger.Functions)
            {
                function.FunctionName = await _functionService.GetFunctionName(function.FunctionId, language);
                function.Description = await _functionService.GetFunctionDescription(function.FunctionId, language);
            }

            return custBotMessenger;
        }

        private async Task<IEnumerable<BotFunction>> GetAccountFunctions(int custId, int subAccId, byte customerLevel, IEnumerable<BotFunction> functions)
        {
            functions = functions.Concat(await _functionService.GetReportAndCommonFunctions());

            if ((CustomerLevel)customerLevel != CustomerLevel.Agent)
            {
                functions = functions.Where(f => f.FunctionId != FunctionIds.MyWatchlist);
            }

            if (subAccId > 0)
            {
                var customerStatus = await _customerService.GetCustomerStatus(custId, subAccId);

                if (!customerStatus.HasReportPermission)
                {
                    functions = functions.Where(f =>
                        f.FunctionId != FunctionIds.Outstanding
                        && f.FunctionId != FunctionIds.TodayWinLoss
                        && f.FunctionId != FunctionIds.YesterdayWinLoss
                        && f.FunctionId != FunctionIds.YesterdayWinLossNotification);
                }

                if (!customerStatus.HasMemberInformationPermission)
                {
                    functions = functions.Where(f =>
                        f.FunctionId != FunctionIds.ProblemAccount
                        && f.FunctionId != FunctionIds.MemberWinLossLimit
                        && f.FunctionId != FunctionIds.BalanceInfo
                        && f.FunctionId != FunctionIds.MyWatchlist);
                }
            }

            return functions;
        }

        private async Task<CustBotMessenger> GetVerifyingMessenger(int custId, int subAccId)
        {
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, !_webAPIConfiguration.IsProduction, true, false);
            return (await _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(criteria).ConfigureAwait(false)).firstResultSet.FirstOrDefault();
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
                account.Functions = await GetAccountFunctions(account.CustId, account.SubAccId, account.CustomerLevel, functions);
            }

            return account;
        }

        private async Task SendWelcomeMessage(int custId, int subAccId, string username)
        {
            var messenger = await GetCustBotMessenger(custId, subAccId);
            var text = _messageBuilder.BuildWelcomeMessage(messenger.Language, username);
            var rootActionButtons = await _actionService.BuildRootActionButtons(await GetCustBotAccount(messenger.ChatId, messenger.BotId), messenger.System);
            var message = _messageBuilder.BuildMessage(text, rootActionButtons, messenger.Language);
            await _messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);
        }
    }
}