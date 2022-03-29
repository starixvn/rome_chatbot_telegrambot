using Fanex.Caching;
using Fanex.Data.Repository;
using System;
using System.Threading.Tasks;
using TelegramBot.Common.Configurations;
using TelegramBot.Common.Constants;
using TelegramBot.Common.Extensions;
using TelegramBot.Common.Models;
using TelegramBot.Common.Resources;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Criteria;

namespace TelegramBot.Infrastructure.Setting
{
    public interface ISettingService
    {
        Task<CustBot> GetCustBotAsync(long chatId);

        Task<Result> StartAsync(long messengerBotId, long chatId, int custId, int subAccId, string messengerName, string language);

        Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp);

        Task<Result> UnlinkAsync(int botId, int custId, int subAccId);
    }

    public class SettingService : ISettingService
    {
        private readonly WebApiConfiguration _webApiConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IBotService _botService;
        private readonly ICacheService _cacheService;
        private readonly ISenderService _senderService;

        public SettingService(WebApiConfiguration webApiConfiguration, IDynamicRepository dynamicRepository, IBotService botService, ICacheService cacheService, ISenderService senderService)
        {
            _webApiConfiguration = webApiConfiguration;
            _dynamicRepository = dynamicRepository;
            _botService = botService;
            _cacheService = cacheService;
            _senderService = senderService;
        }

        public async Task<Result> StartAsync(long messengerBotId, long chatId, int custId, int subAccId, string messengerName, string language)
        {
            var bot = await _botService.GetAsync(messengerBotId);
            var criteria = new CustBotAccountInsCriteria(bot.BotId, chatId, custId, subAccId, messengerName, language);
            await _dynamicRepository.ExecuteAsync(criteria);
            if (criteria.ErrCode != ErrorCodes.Success)
            {
                return new Result(criteria.ErrCode, criteria.ErrMsg);
            }

            var otp = ThreadSafeRandom.Next(0, 999999).ToString("D6");
            var cacheKey = $"{custId}_{subAccId}";
            var cacheItemOptions = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_webApiConfiguration.OtpExpirationInMinutes));
            await _cacheService.SetAsync(cacheKey, otp, cacheItemOptions);
            var message = string.Format(ResourceHelper.Get(nameof(SharedResource.Setting_OTP_Help)), otp);
            await _senderService.SendMessage(messengerBotId, chatId, message);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> LinkAsync(int botId, int custId, int subAccId, string otp)
        {
            var cacheKey = $"{custId}_{subAccId}";
            var expectedOtp = await _cacheService.GetAsync<string>(cacheKey);
            if (string.IsNullOrEmpty(expectedOtp))
            {
                return new Result(-1, "OTP is expired");
            }

            if (otp != expectedOtp)
            {
                return new Result(-2, "Invalid OTP");
            }

            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, true, true);
            await _dynamicRepository.ExecuteAsync(criteria);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<Result> UnlinkAsync(int botId, int custId, int subAccId)
        {
            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, false, false);
            await _dynamicRepository.ExecuteAsync(criteria);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<CustBot> GetCustBotAsync(long chatId)
        {
            var criteria = new GetCustBotCriteria(chatId);
            return await _dynamicRepository.GetAsync<CustBot>(criteria);
        }
    }
}
