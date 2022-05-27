using Alpha.API.Client.Models.WinLoss;
using Fanex.Data.Repository;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.PersonalMessages;
using ClientServices = Alpha.API.Client.Services; 

namespace TelegramBot.Importer.Services;

public interface IYesterdayWinLossService
{
    string BuildMessage(WinLossBase winloss);

    Task<Result> Import(
        IEnumerable<ImportYesterdayWinLoss> notifications,
        string environment,
        int maxUserId);
}

public class YesterdayWinLossService : IYesterdayWinLossService
{
    private readonly ClientServices.IWinLossService winLossService;
    private readonly IDynamicRepository dynamicRepository;

    public YesterdayWinLossService(
        ClientServices.IWinLossService winLossService,
        IDynamicRepository dynamicRepository)
    {
        this.winLossService = winLossService;
        this.dynamicRepository = dynamicRepository;
    }

    public string BuildMessage(WinLossBase winloss)
    {
        throw new NotImplementedException();
    }

    public Task<Result> Import(
        IEnumerable<ImportYesterdayWinLoss> notifications,
        string environment,
        int maxUserId)
    {
        throw new NotImplementedException();
    }
}