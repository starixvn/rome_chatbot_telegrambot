using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using TelegramBot.Infrastructure.Bot;

namespace TelegramBot.Infrastructure.Message
{
    public interface IBotClientFactory
    {
        ITelegramBotClient GetOrCreate(long messengerBotId);
    }

    public class BotClientFactory : IBotClientFactory
    {
        private readonly IBotService botService;
        private readonly Dictionary<long, ITelegramBotClient> botClients = new();

        public BotClientFactory(IBotService botService)
        {
            this.botService = botService;
        }

        public ITelegramBotClient GetOrCreate(long messengerBotId)
        {
            if (botClients.ContainsKey(messengerBotId))
            {
                return botClients[messengerBotId];
            }

            
            lock (botClients)
            {
                if (botClients.ContainsKey(messengerBotId))
                {
                    return botClients[messengerBotId];
                }

                var bots = botService.GetAllAsync().GetAwaiter().GetResult();
                var bot = bots.First(x => x.MessengerBotId == messengerBotId);
                var botClient = new TelegramBotClient(bot.AccessToken);
                botClients.Add(messengerBotId, botClient);
                return botClient;
            }
        }
    }
}
