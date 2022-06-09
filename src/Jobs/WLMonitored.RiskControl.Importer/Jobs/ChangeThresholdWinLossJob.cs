using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.RiskControl.Importer.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Extensions;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;

namespace WLMonitored.RiskControl.Importer.Jobs;

public sealed class ChangeThresholdWinLossJob : ChangeWinLossBaseJob
{
    public ChangeThresholdWinLossJob(
        Dictionary<string, ScheduleJob> scheduleJobDict,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IGlobalSettingService globalSettingService,
        IWinLossService winLossService,
        IPlayerHistoryService playerHistoryService)
        : base(
            scheduleJobDict.ContainsKey(nameof(ChangeThresholdWinLossJob))
                ? scheduleJobDict[nameof(ChangeThresholdWinLossJob)]
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
        var winLossThresholds = await winLossService.GetChangedWinLossThresholds(
            globalSetting.CreditSuperUUS,
            scheduleJob.IsUAT,
            globalSetting.LastScanSettingDate);

        if (!winLossThresholds.Any())
        {
            return false;
        }

        var custIds = winLossThresholds.Select(wl => wl.CustId);
        var accumulatedWinLossList = await winLossService.GetAccumulatedWinLoss(
            custIds.JoinWithComma(),
            DateTime.Today);

        if (!accumulatedWinLossList.Any())
        {
            return false;
        }

        var reachPlayerHistoryParams =
            from wlThreshold in winLossThresholds
            join accumWL in accumulatedWinLossList on wlThreshold.CustId equals accumWL.CustId
            where IsReachWinning(accumWL, wlThreshold) || IsReachLosing(accumWL, wlThreshold)
            select BuildReachPlayerHistoryParam(accumWL, wlThreshold);

        var lastScanDate = (from wlThreshold in winLossThresholds
            let max = Max(wlThreshold.WinningUpdateTime, wlThreshold.LosingUpdateTime)
            select max).Max();

        await InsertPlayerHistoryInCaseOfChangeSetting(
            reachPlayerHistoryParams,
            lastScanDate,
            globalSetting.GetWLMonitoredAccountWithEnvironment());

        return true;
    }

    private static DateTime? Max(DateTime? winningUpdateTime, DateTime? losingUpdateTime)
    {
        if (!winningUpdateTime.HasValue && !losingUpdateTime.HasValue)
        {
            return null;
        }

        if (!winningUpdateTime.HasValue)
        {
            return losingUpdateTime;
        }

        if (!losingUpdateTime.HasValue)
        {
            return winningUpdateTime;
        }

        return winningUpdateTime.Value > losingUpdateTime.Value ? winningUpdateTime : losingUpdateTime;
    }
}
