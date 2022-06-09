using System.Text;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.RiskControl.Importer.Models;
using WLMonitored.RiskControl.Importer.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;

namespace WLMonitored.RiskControl.Importer.Jobs;

public sealed class ChangeAccumulatedWinLossJob : ChangeWinLossBaseJob
{
    public ChangeAccumulatedWinLossJob(
        Dictionary<string, ScheduleJob> scheduleJobDict,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IGlobalSettingService globalSettingService,
        IWinLossService winLossService,
        IPlayerHistoryService playerHistoryService)
        : base(
            scheduleJobDict.ContainsKey(nameof(ChangeAccumulatedWinLossJob))
                ? scheduleJobDict[nameof(ChangeAccumulatedWinLossJob)]
                : new ScheduleJob(),
            loggerService,
            maintenanceService,
            umClientConfiguration,
            globalSettingService,
            winLossService,
            playerHistoryService)
        {
        }

    public override async Task<bool> ExecuteJob(GlobalSetting globalSetting)
    {
        var accumulatedWinLossList = await winLossService.GetLatestAccumulatedWinLoss(globalSetting.LastCheckTime, scheduleJob.BatchSize);

        if (!accumulatedWinLossList.Any())
        {
            return false;
        }

        var reachPlayerHistoryParams = Enumerable.Empty<ReachPlayerHistoryParam>();

        var (lastUpdatedTime, custIds) = GetLastUpdatedTimeAndCustIds(accumulatedWinLossList);
        var winLossThresholds = await winLossService.GetWinLossThresholds(
            custIds,
            globalSetting.CreditSuperUUS,
            scheduleJob.IsUAT);
        var hasData = winLossThresholds.Any();

        if (hasData)
        {
            reachPlayerHistoryParams =
                from accumWL in accumulatedWinLossList
                join wlThreshold in winLossThresholds on accumWL.CustId equals wlThreshold.CustId
                where IsReachWinning(accumWL, wlThreshold) || IsReachLosing(accumWL, wlThreshold)
                select BuildReachPlayerHistoryParam(accumWL, wlThreshold);
        }

        await InsertPlayerHistoryInCaseOfReachThreshold(
            reachPlayerHistoryParams,
            lastUpdatedTime,
            globalSetting.GetWLMonitoredAccountWithEnvironment());

        return hasData;
    }

    private static Tuple<DateTime, string> GetLastUpdatedTimeAndCustIds(IEnumerable<AccumulatedWinLoss> accumulatedWinLossList)
    {
        var lastUpdatedTime = DateTime.MinValue;
        var custIds = new StringBuilder();

        foreach (var item in accumulatedWinLossList)
        {
            if (lastUpdatedTime < item.UpdatedTime)
            {
                lastUpdatedTime = item.UpdatedTime;
            }

            custIds
                .Append(item.CustId)
                .Append(',');
        }

        return new Tuple<DateTime, string>(lastUpdatedTime, custIds.ToString().TrimEnd(','));
    }
}
