using Fanex.Data.Repository;
using System.Threading.Tasks;
using TelegramBot.Common.Models;
using TelegramBot.Infrastructure.Setting.Criteria;

namespace TelegramBot.Infrastructure.Setting
{
    public interface ISettingService
    {
        Task<CustBot> GetCustBotAsync(long chatId);
    }

    public class SettingService : ISettingService
    {
        private readonly IDynamicRepository dynamicRepository;

        public SettingService(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<CustBot> GetCustBotAsync(long chatId)
        {
            var criteria = new GetCustBotCriteria(chatId);
            return await dynamicRepository.GetAsync<CustBot>(criteria);
        }
    }
}
