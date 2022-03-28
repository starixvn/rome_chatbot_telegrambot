using Fanex.Data.Repository;
using System.Threading.Tasks;
using TelegramBot.Common.Models;
using TelegramBot.Infrastructure.Setting.Criteria;

namespace TelegramBot.Infrastructure.Setting
{
    public interface ISettingService
    {
        Task<CustBot> GetCustBotAsync(long chatId);

        Task<Result> InsertCustBotAsync(int botId, long chatId, int custId, int subAccId, string messengerName, string language);
    }

    public class SettingService : ISettingService
    {
        private readonly IDynamicRepository dynamicRepository;

        public SettingService(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<Result> InsertCustBotAsync(int botId, long chatId, int custId, int subAccId, string messengerName, string language)
        {
            var criteria = new CustBotAccountInsCriteria(botId, chatId, custId, subAccId, messengerName, language);
            await dynamicRepository.ExecuteAsync(criteria);
            return new Result(criteria.ErrCode, criteria.ErrMsg);
        }

        public async Task<CustBot> GetCustBotAsync(long chatId)
        {
            var criteria = new GetCustBotCriteria(chatId);
            return await dynamicRepository.GetAsync<CustBot>(criteria);
        }
    }
}
