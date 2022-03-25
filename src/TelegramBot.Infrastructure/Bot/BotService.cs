using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Common.Entities;
using TelegramBot.Infrastructure.Bot.Criteria;

namespace TelegramBot.Infrastructure.Bot
{
    public interface IBotService
    {
        Task<List<BotEntity>> GetAllAsync();
        Task<BotEntity> GetAsync(string system, bool isTest);
        Task<BotEntity> GetAsync(long messengerBotId);
    }

    public class BotService : IBotService
    {
        private readonly IDynamicRepository _dynamicRepository;
        private List<BotEntity> _bots;

        public BotService(IDynamicRepository dynamicRepository)
        {
            _dynamicRepository = dynamicRepository;
        }

        public async Task<List<BotEntity>> GetAllAsync()
        {
            if (_bots == null)
            {
                _bots = (await _dynamicRepository.FetchAsync<BotEntity>(new GetAllCriteria()).ConfigureAwait(false))
                    .Where(bot => bot.Status)
                    .ToList();
                _bots.ForEach(bot =>
                {
                    bot.MessengerBotId = long.Parse(bot.AccessToken.Split(":").First());
                });
            }

            return _bots;
        }

        public async Task<BotEntity> GetAsync(string system, bool isTest)
        {
            var bots = await GetAllAsync().ConfigureAwait(false);
            return bots.FirstOrDefault(x => x.System == system && x.IsTest == isTest);
        }

        public async Task<BotEntity> GetAsync(long messengerBotId)
        {
            var bots = await GetAllAsync().ConfigureAwait(false);
            return bots.FirstOrDefault(x => x.MessengerBotId == messengerBotId);
        }
    }
}
