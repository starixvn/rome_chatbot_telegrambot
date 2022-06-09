using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.RiskControl.Importer.Models;
using WLMonitored.RiskControl.Importer.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Jobs;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;

namespace WLMonitored.RiskControl.Importer.Jobs;

public class ChangeWinLossBaseJob : IntervalBackgroundService
{
    private const byte NotificationTypeReachThreshold = 1;
    private const byte NotificationTypeChangeSetting = 2;
    private const byte ReachTypeWinning = 1;
    private const byte ReachTypeLosing = 2;

    protected readonly IWinLossService winLossService;
    protected readonly IPlayerHistoryService playerHistoryService;

    protected ChangeWinLossBaseJob(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IGlobalSettingService globalSettingService,
        IWinLossService winLossService,
        IPlayerHistoryService playerHistoryService)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration, globalSettingService)
    {
        this.winLossService = winLossService;
        this.playerHistoryService = playerHistoryService;
    }
    
    public override Task<bool> ExecuteJob(GlobalSetting globalSetting)
        => throw new NotImplementedException();

    protected async Task InsertPlayerHistoryInCaseOfReachThreshold(
        IEnumerable<ReachPlayerHistoryParam> reachPlayerHistoryParams,
        DateTime lastCheckTime,
        string serviceName)
    {
        try
        {
            await playerHistoryService.Insert(
                reachPlayerHistoryParams,
                NotificationTypeReachThreshold,
                lastCheckTime,
                null,
                serviceName);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, scheduleJob, nameof(InsertPlayerHistoryInCaseOfReachThreshold));
        }
    }

    protected async Task InsertPlayerHistoryInCaseOfChangeSetting(
        IEnumerable<ReachPlayerHistoryParam> reachPlayerHistoryParams,
        DateTime? lastScanDate,
        string serviceName)
    {
        try
        {
            await playerHistoryService.Insert(
                reachPlayerHistoryParams,
                NotificationTypeChangeSetting,
                null,
                lastScanDate,
                serviceName);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, scheduleJob, nameof(InsertPlayerHistoryInCaseOfReachThreshold));
        }
    }

    protected static ReachPlayerHistoryParam BuildReachPlayerHistoryParam(
        AccumulatedWinLoss accumulatedWinLoss,
        WinLossThreshold winLossThreshold)
        => new(
            accumulatedWinLoss.CustId,
            accumulatedWinLoss.AccumWL,
            winLossThreshold.SportType,
            winLossThreshold.SettingType,
            winLossThreshold.WinningThreshold,
            winLossThreshold.LosingThreshold,
            IsReachWinning(accumulatedWinLoss, winLossThreshold) ? ReachTypeWinning : ReachTypeLosing);

    protected static bool IsReachWinning(AccumulatedWinLoss accumulatedWinLoss, WinLossThreshold winLossThreshold)
        => winLossThreshold.WinningThreshold.HasValue
        && accumulatedWinLoss.AccumWL >= winLossThreshold.WinningThreshold.Value;

    protected static bool IsReachLosing(AccumulatedWinLoss accumulatedWinLoss, WinLossThreshold winLossThreshold)
        => winLossThreshold.LosingThreshold.HasValue
        && accumulatedWinLoss.AccumWL <= winLossThreshold.LosingThreshold.Value * -1;
}
