using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.RiskControl.Importer.Models;
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
        var lastScanSettingDate = globalSetting.LastScanSettingDate;
        var winLossThresholds = await winLossService.GetChangedWinLossThresholds(
            globalSetting.CreditSuperUUS,
            scheduleJob.IsUAT,
            lastScanSettingDate);

        if (!winLossThresholds.Any())
        {
            return false;
        }

        var (custIds, agentIds, winLossThresholdDict) = GetMemberAndAgentCustIds(winLossThresholds);
        var accumulatedWinLossList = await winLossService.GetAccumulatedWinLoss(
            custIds.JoinWithComma(),
            agentIds.JoinWithComma(),
            DateTime.Today);

        if (!accumulatedWinLossList.Any())
        {
            return false;
        }

        var reachPlayerHistoryParams = BuildReachPlayerHistoryParams(
            accumulatedWinLossList,
            winLossThresholdDict,
            lastScanSettingDate);
        await InsertPlayerHistoryInCaseOfChangeSetting(
            reachPlayerHistoryParams,
            DateTime.Now,
            globalSetting.GetWLMonitoredAccountWithEnvironment());

        return true;
    }

    private static Tuple<IEnumerable<int>, IEnumerable<int>, IDictionary<int, WinLossThreshold>> GetMemberAndAgentCustIds(
        IEnumerable<WinLossThreshold> winLossThresholds)
    {
        const byte GlobalThreshold = 1;
        const byte PersonalThreshold = 2;

        IList<int> members = new List<int>();
        IList<int> agents = new List<int>();
        IDictionary<int, WinLossThreshold> winLossThresholdDict = new Dictionary<int, WinLossThreshold>();

        foreach (var winloss in winLossThresholds)
        {
            if (winloss.SettingType == GlobalThreshold)
            {
                agents.Add(winloss.CustId);
            }
            else if (winloss.SettingType == PersonalThreshold)
            {
                members.Add(winloss.CustId);
            }

            winLossThresholdDict[winloss.CustId] = winloss;
        }

        return Tuple.Create<IEnumerable<int>, IEnumerable<int>, IDictionary<int, WinLossThreshold>>(members, agents, winLossThresholdDict);
    }
}
