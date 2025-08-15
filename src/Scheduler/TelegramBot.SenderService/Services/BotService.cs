using System.Diagnostics.CodeAnalysis;
using Fanex.Caching;
using Fanex.Data.Repository;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;
using TelegramBot.Utilities;

namespace TelegramBot.SenderService.Services;

public interface IBotService
{
    Task SendMessage(int botId, long chatId, string message);
}

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
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
            var botClient = TelegramBotUtils.InitBotClient(bot.AccessToken, appSettings.ProxyAddress);
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
    {
        return await cacheService.GetOrSetAsync(
            AllBotCacheKey,
            () => dynamicRepository.FetchAsync<Bot>(new GetAllBotCriteria(appSettings.IsTest)),
            new CacheItemOptions().SetAbsoluteExpiration(new TimeSpan(1, 0, 0)));
    }
}