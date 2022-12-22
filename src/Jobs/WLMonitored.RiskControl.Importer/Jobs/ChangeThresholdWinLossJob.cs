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

        var (custIds, agentIds, smaIds, winLossThresholdDict) = GetSMAMemberAndAgentCustIds(winLossThresholds);
        var accumulatedWinLossList = await winLossService.GetAccumulatedWinLoss(
            custIds.JoinWithComma(),
            agentIds.JoinWithComma(),
            smaIds.JoinWithComma(),
            DateTime.Today);

        var hasData = accumulatedWinLossList.Any();
        var reachPlayerHistoryParams = hasData
            ? BuildReachPlayerHistoryParams(accumulatedWinLossList, winLossThresholdDict, lastScanSettingDate)
            : Enumerable.Empty<ReachPlayerHistoryParam>();

        await InsertPlayerHistoryInCaseOfChangeSetting(
            reachPlayerHistoryParams,
            DateTime.Now,
            globalSetting.GetWLMonitoredAccountWithEnvironment());

        return hasData;
    }

    private static Tuple<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>, IDictionary<int, WinLossThreshold>> GetSMAMemberAndAgentCustIds(
        IEnumerable<WinLossThreshold> winLossThresholds)
    {
        const byte GlobalThreshold = 1;
        const byte PersonalThreshold = 2;
        const byte SMAThreshold = 3;

        IList<int> members = new List<int>();
        IList<int> agents = new List<int>();
        IList<int> smaList = new List<int>();
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
            else if (winloss.SettingType == SMAThreshold)
            {
                smaList.Add(winloss.CustId);
            }

            winLossThresholdDict[winloss.CustId] = winloss;
        }

        return Tuple.Create<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>, IDictionary<int, WinLossThreshold>>(
            members, agents, smaList, winLossThresholdDict);
    }
}
