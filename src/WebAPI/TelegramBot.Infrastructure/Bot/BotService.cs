using Fanex.Data.Repository;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBot.Infrastructure.Bot.Criteria;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Utilities;

namespace TelegramBot.Infrastructure.Bot
{
    public interface IBotService
    {
        Task<List<BotEntity>> GetAllAsync();

        Task<BotEntity> GetAsync(string system);

        Task<BotEntity> GetAsync(long messengerBotId);

        Task<BotEntity> GetAsync(int botId);

        Task<Result> SetWebhook(int botId, string url);

        Task<Result> RemoveWebhook(int botId);
    }

    public class BotService : IBotService
    {
        private readonly IDynamicRepository _dynamicRepository;
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private List<BotEntity> _botEntities;
        private readonly UpdateType[] _allowedUpdates = new[]
        {
            UpdateType.Message,
            UpdateType.CallbackQuery
        };

        public BotService(IDynamicRepository dynamicRepository, WebAPIConfiguration webAPIConfiguration)
        {
            _dynamicRepository = dynamicRepository;
            _webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<List<BotEntity>> GetAllAsync()
        {
            if (_botEntities == null)
            {
                _botEntities = (await _dynamicRepository.FetchAsync<BotEntity>(new GetAllCriteria(!_webAPIConfiguration.IsProduction)).ConfigureAwait(false)).ToList();
                _botEntities.ForEach(botEntity =>
                {
                    botEntity.MessengerBotId = long.Parse(botEntity.AccessToken.Split(":").First());
                });
            }

            return _botEntities;
        }

        public async Task<BotEntity> GetAsync(string system)
        {
            var botEntities = await GetAllAsync();
            return botEntities.FirstOrDefault(x => x.System == system);
        }

        public async Task<BotEntity> GetAsync(long messengerBotId)
        {
            var botEntities = await GetAllAsync();
            return botEntities.FirstOrDefault(x => x.MessengerBotId == messengerBotId);
        }

        public async Task<BotEntity> GetAsync(int botId)
        {
            var botEntities = await GetAllAsync();
            return botEntities.FirstOrDefault(x => x.BotId == botId);
        }

        public async Task<Result> SetWebhook(int botId, string url)
        {
            var bot = await GetAsync(botId);
            var client = TelegramBotUtils.InitBotClient(bot.AccessToken, _webAPIConfiguration.ProxyAddress);
            await client.SetWebhookAsync(url: $"{url}/bot/{bot.AccessToken}", allowedUpdates: _allowedUpdates);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> RemoveWebhook(int botId)
        {
            var bot = await GetAsync(botId);
            var client = TelegramBotUtils.InitBotClient(bot.AccessToken, _webAPIConfiguration.ProxyAddress);
            await client.DeleteWebhookAsync();
            return Result.CreateSuccessfulResult();
        }
    }
}