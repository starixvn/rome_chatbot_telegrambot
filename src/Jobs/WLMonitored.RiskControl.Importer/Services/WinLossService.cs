using Fanex.Data.Repository;
using WLMonitored.RiskControl.Importer.Criteria;
using WLMonitored.RiskControl.Importer.Models;
using WLMonitored.Shared.Criteria;
using WLMonitored.Shared.Models;

namespace WLMonitored.RiskControl.Importer.Services;

public interface IWinLossService
{
    Task<IEnumerable<AccumulatedWinLoss>> GetLatestAccumulatedWinLoss(DateTime lastCheckTime, int batchSize);

    Task<IEnumerable<WinLossThreshold>> GetWinLossThresholds(string custIds, string superUUS, bool isTest);

    Task<IEnumerable<WinLossThreshold>> GetChangedWinLossThresholds(string superUUS, bool isTest, DateTime lastScanDate);

    Task<IEnumerable<AccumulatedWinLoss>> GetAccumulatedWinLoss(string custIds, string recommends, string smaIds, DateTime currentDate);
}

public sealed class WinLossService : IWinLossService
{
    private readonly IDynamicRepository dynamicRepository;

    public WinLossService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public Task<IEnumerable<AccumulatedWinLoss>> GetLatestAccumulatedWinLoss(DateTime lastCheckTime, int batchSize)
        => dynamicRepository.FetchAsync<AccumulatedWinLoss>(new GetLatestAccumulatedWinLossCriteria(lastCheckTime, batchSize));

    public Task<IEnumerable<WinLossThreshold>> GetWinLossThresholds(string custIds, string superUUS, bool isTest)
        => dynamicRepository.FetchAsync<WinLossThreshold>(new GetWinLossSettingCriteria(custIds, superUUS, isTest));

    public Task<IEnumerable<WinLossThreshold>> GetChangedWinLossThresholds(string superUUS, bool isTest, DateTime lastScanDate)
        => dynamicRepository.FetchAsync<WinLossThreshold>(new GetChangedWinLossThresholdCriteria(superUUS, isTest, lastScanDate));

    public Task<IEnumerable<AccumulatedWinLoss>> GetAccumulatedWinLoss(string custIds, string recommends, string smaIds, DateTime currentDate)
        => dynamicRepository.FetchAsync<AccumulatedWinLoss>(
            new GetCreditAccumulatedWinLossCriteria(custIds, smaIds, currentDate, recommends));
}