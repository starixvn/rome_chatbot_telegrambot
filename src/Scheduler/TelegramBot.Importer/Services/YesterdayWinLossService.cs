using Alpha.API.Client.Models.WinLoss;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.YesterdayWinLoss;
using TelegramBot.Importer.Models;
using ClientServices = Alpha.API.Client.Services;

namespace TelegramBot.Importer.Services;

public interface IYesterdayWinLossService
{
    Task<WinLossBase> GetWinLoss(int custId, byte custLevel, DateTime winLossDate);

    Task<Result> Import(
        IEnumerable<Notification> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId);
}

public class YesterdayWinLossService : NotificationServiceBase, IYesterdayWinLossService
{
    private readonly ClientServices.IWinLossService winLossService;

    public YesterdayWinLossService(
        ClientServices.IWinLossService winLossService,
        IDynamicRepository dynamicRepository) : base(dynamicRepository)
        => this.winLossService = winLossService;

    public Task<WinLossBase> GetWinLoss(int custId, byte custLevel, DateTime winLossDate)
        => winLossService.GetWinLoss(custId, custLevel, winLossDate);

    public Task<Result> Import(
        IEnumerable<Notification> notifications,
        DateTime winLossDate,
        string environment,
        int maxUserId)
        => ExecuteNonQuery(new ImportYesterdayWinLossCommand(
            BuildXmlData(notifications),
            winLossDate,
            environment,
            maxUserId));
}