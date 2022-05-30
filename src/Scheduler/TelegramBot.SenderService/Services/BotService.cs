using Fanex.Caching;
using Fanex.Data.Repository;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.Services;

public interface IBotService
{
    Task SendMessage(int botId, long chatId, string message);
}

public class BotService : IBotService
{
    private readonly string AllBotCacheKey = "TelegramBot.SenderService.Services.AllBot";
    private readonly AppSettings appSettings;
    private readonly ICacheService cacheService;
    private readonly IDynamicRepository dynamicRepository;
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private readonly IDictionary<int, ITelegramBotClient> botClients = new Dictionary<int, ITelegramBotClient>();

    public BotService(
        AppSettings appSettings,
        ICacheService cacheService,
        IDynamicRepository dynamicRepository)
    {
        this.appSettings = appSettings;
        this.cacheService = cacheService;
        this.dynamicRepository = dynamicRepository;
    }

    public async Task SendMessage(int botId, long chatId, string message)
    {
        var botClient = await GetBotClient(botId);
        await botClient.SendTextMessageAsync(chatId, message, ParseMode.Html);
    }

    private async Task<ITelegramBotClient> GetBotClient(int botId)
    {
        if (botClients.ContainsKey(botId))
        {
            return botClients[botId];
        }

        await semaphore.WaitAsync();
        try
        {
            var bot = await GetBot(botId);
            var botClient = new TelegramBotClient(bot.AccessToken);
            botClients.Add(botId, botClient);

            return botClient;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<Bot> GetBot(int botId)
        => (await GetBots()).FirstOrDefault(bot => bot.BotId == botId);

    private async Task<IEnumerable<Bot>> GetBots()
        => await cacheService.GetOrSetAsync(AllBotCacheKey, () => dynamicRepository.FetchAsync<Bot>(new GetAllBotCriteria(appSettings.IsTest)));

}