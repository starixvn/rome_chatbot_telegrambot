using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Client.Models;
using TelegramBot.Infrastructure.Bot.Criteria;

namespace TelegramBot.Infrastructure.Bot
{
    public interface IBotService
    {
        Task<List<BotEntity>> GetAllAsync();
        Task<BotEntity> GetAsync(string system, bool isTest);
        Task<BotEntity> GetAsync(long messengerBotId);
        Task<BotEntity> GetAsync(int botId);
    }

    public class BotService : IBotService
    {
        private readonly IDynamicRepository _dynamicRepository;
        private List<BotEntity> _botEntities;

        public BotService(IDynamicRepository dynamicRepository)
        {
            _dynamicRepository = dynamicRepository;
        }

        public async Task<List<BotEntity>> GetAllAsync()
        {
            if (_botEntities == null)
            {
                _botEntities = (await _dynamicRepository.FetchAsync<BotEntity>(new GetAllCriteria()).ConfigureAwait(false))
                    .Where(bot => bot.Status)
                    .ToList();
                _botEntities.ForEach(botEntity =>
                {
                    botEntity.MessengerBotId = long.Parse(botEntity.AccessToken.Split(":").First());
                });
            }

            return _botEntities;
        }

        public async Task<BotEntity> GetAsync(string system, bool isTest)
        {
            var botEntities = await GetAllAsync().ConfigureAwait(false);
            return botEntities.FirstOrDefault(x => x.System == system && x.IsTest == isTest);
        }

        public async Task<BotEntity> GetAsync(long messengerBotId)
        {
            var botEntities = await GetAllAsync().ConfigureAwait(false);
            return botEntities.FirstOrDefault(x => x.MessengerBotId == messengerBotId);
        }

        public async Task<BotEntity> GetAsync(int botId)
        {
            var botEntities = await GetAllAsync().ConfigureAwait(false);
            return botEntities.FirstOrDefault(x => x.BotId == botId);
        }
    }
}
