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
    }

    public class BotService : IBotService
    {
        private readonly IDynamicRepository dynamicRepository;
        private List<BotEntity> bots;

        public BotService(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<List<BotEntity>> GetAllAsync()
        {
            if (bots == null)
            {
                bots = (await dynamicRepository.FetchAsync<BotEntity>(new GetAllCriteria()).ConfigureAwait(false))
                    .Where(bot => bot.Status)
                    .ToList();
                bots.ForEach(bot =>
                {
                    bot.MessengerBotId = long.Parse(bot.AccessToken.Split(":").First());
                });
            }

            return bots;
        }

        public async Task<BotEntity> GetAsync(string system, bool isTest)
        {
            var bots = await GetAllAsync().ConfigureAwait(false);
            return bots.FirstOrDefault(x => x.System == system && x.IsTest == isTest);
        }
    }
}
