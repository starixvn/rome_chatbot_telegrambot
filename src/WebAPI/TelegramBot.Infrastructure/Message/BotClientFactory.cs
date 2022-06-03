using Telegram.Bot;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Utilities;

namespace TelegramBot.Infrastructure.Message
{
    public interface IBotClientFactory
    {
        Task<ITelegramBotClient> GetOrCreate(int botId);
    }

    public class BotClientFactory : IBotClientFactory
    {
        private readonly IBotService botService;
        private readonly Dictionary<int, ITelegramBotClient> botClients = new();
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly WebAPIConfiguration webAPIConfiguration;

        public BotClientFactory(IBotService botService, WebAPIConfiguration webAPIConfiguration)
        {
            this.botService = botService;
            this.webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<ITelegramBotClient> GetOrCreate(int botId)
        {
            if (botClients.ContainsKey(botId))
            {
                return botClients[botId];
            }

            await semaphore.WaitAsync();
            try
            {
                var bot = await botService.GetAsync(botId);
                var botClient = TelegramBotUtils.InitBotClient(bot.AccessToken, webAPIConfiguration.ProxyAddress);
                botClients.Add(botId, botClient);

                return botClient;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
