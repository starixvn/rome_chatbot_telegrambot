using System.Collections.Generic;
using Telegram.Bot;
using TelegramBot.Infrastructure.Bot;

namespace TelegramBot.Infrastructure.Message
{
    public interface IBotClientFactory
    {
        ITelegramBotClient GetOrCreate(int botId);
    }

    public class BotClientFactory : IBotClientFactory
    {
        private readonly IBotService botService;
        private readonly Dictionary<long, ITelegramBotClient> botClients = new();

        public BotClientFactory(IBotService botService)
        {
            this.botService = botService;
        }

        public ITelegramBotClient GetOrCreate(int botId)
        {
            if (botClients.ContainsKey(botId))
            {
                return botClients[botId];
            }


            lock (botClients)
            {
                if (botClients.ContainsKey(botId))
                {
                    return botClients[botId];
                }

                var bot = botService.GetAsync(botId).GetAwaiter().GetResult();
                var botClient = new TelegramBotClient(bot.AccessToken);
                botClients.Add(botId, botClient);
                return botClient;
            }
        }
    }
}
