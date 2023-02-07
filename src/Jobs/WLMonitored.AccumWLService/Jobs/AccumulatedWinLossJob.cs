using Microsoft.Extensions.Options;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.AccumWLService.Models;
using WLMonitored.AccumWLService.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Constants;
using WLMonitored.Shared.Extensions;
using WLMonitored.Shared.Jobs;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;

namespace WLMonitored.AccumWLService.Jobs;

public sealed class AccumulateWinLossJob : IntervalBackgroundService
{
    private readonly ITicketDetailService ticketDetailService;
    private readonly IWinLossService winLossService;
    private readonly IBetTypeService betTypeService;

    public AccumulateWinLossJob(
        IOptions<ScheduleJob> options,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IGlobalSettingService globalSettingService,
        ITicketDetailService ticketDetailService,
        IWinLossService winLossService,
        IBetTypeService betTypeService)
        : base(options.Value, loggerService, maintenanceService, umClientConfiguration, globalSettingService)
    {
        this.ticketDetailService = ticketDetailService;
        this.winLossService = winLossService;
        this.betTypeService = betTypeService;
    }

    public override async Task<bool> ExecuteJob(GlobalSetting globalSetting)
    {
        var ticketDetails = await ticketDetailService.GetTicketDetails(globalSetting.LastId, scheduleJob.BatchSize);

        if (!ticketDetails.Any())
        {
            return false;
        }

        var lastId = ticketDetails.Max(t => t.Id);
        var accumulatedWinLossParams = new List<AccumulatedWinLossParam>();

        var (creditBetTypes, depositBetTypes) = await betTypeService.GetBetTypes();
        var superUUS = GetSuperUUS(globalSetting);

        var ticketDetailsGroupByDate = ticketDetails
            .GroupBy(t => t.SettlementTime.Date)
            .OrderBy(t => t.Key);
        foreach (var groupByDateTickets in ticketDetailsGroupByDate)
        {
            accumulatedWinLossParams.AddRange(await BuildAccumulatedWinLossParams(
                groupByDateTickets,
                creditBetTypes,
                depositBetTypes,
                superUUS,
                scheduleJob.IsUAT));
        }

        try
        {
            await winLossService.UpdateAccumulatedWinLoss(
                accumulatedWinLossParams,
                lastId,
                globalSetting.GetWLMonitoredAccountWithEnvironment());
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, scheduleJob, nameof(winLossService.UpdateAccumulatedWinLoss));
        }

        return true;
    }

    private static string GetSuperUUS(GlobalSetting globalSetting)
    {
        if (string.IsNullOrWhiteSpace(globalSetting.CreditSuperUUS))
        {
            return globalSetting.DepositSuperUUS;
        }

        if (string.IsNullOrWhiteSpace(globalSetting.DepositSuperUUS))
        {
            return globalSetting.CreditSuperUUS;
        }

        return $"{globalSetting.CreditSuperUUS},{globalSetting.DepositSuperUUS}";
    }

    private async Task<IEnumerable<AccumulatedWinLossParam>> BuildAccumulatedWinLossParams(
        IGrouping<DateTime, TicketDetail> groupByDateTickets,
        IDictionary<short, bool> creditBetTypes,
        IDictionary<short, bool> depositBetTypes,
        string superUUS,
        bool isTest)
    {
        var ticketDetailsGroupByCustId = groupByDateTickets.GroupBy(t => t.CustId).ToList();
        var winLossSettings = await GetWinLossSettings(ticketDetailsGroupByCustId, superUUS, isTest);

        if (winLossSettings.Any())
        {
            var settlementDate = groupByDateTickets.Key;
            var accumulatedWinLossList = await GetAccumulatedWinLoss(winLossSettings, settlementDate);

            return
                from tickets in ticketDetailsGroupByCustId
                join wlSetting in winLossSettings on tickets.Key equals wlSetting.CustId
                join accumWL in accumulatedWinLossList on wlSetting.SettingCustId equals accumWL.CustId into leftJoinAccumulatedWinLoss
                from accumulatedWL in leftJoinAccumulatedWinLoss.DefaultIfEmpty()
                select BuildAccumulatedWinLossParam(
                    tickets,
                    wlSetting,
                    accumulatedWL,
                    settlementDate,
                    creditBetTypes,
                    depositBetTypes);
        }

        return Enumerable.Empty<AccumulatedWinLossParam>();
    }

    private Task<IEnumerable<WinLossSetting>> GetWinLossSettings(
        IEnumerable<IGrouping<int, TicketDetail>> ticketDetailsGroupByCustId,
        string superUUS,
        bool isTest)
    {
        var custIds = ticketDetailsGroupByCustId.Select(ticket => ticket.Key);
        var winLossSettings = winLossService.GetWinLossSettings(custIds.JoinWithComma(), superUUS, isTest);

        return winLossSettings;
    }

    private Task<IEnumerable<AccumulatedWinLoss>> GetAccumulatedWinLoss(
        IEnumerable<WinLossSetting> winLossSettings,
        DateTime settlementDate)
    {
        var creditCustIds = new List<int>();
        var creditSMAIds = new List<int>();
        var depositCustIds = new List<int>();

        foreach (var wl in winLossSettings)
        {
            if (wl.IsDeposit)
            {
                depositCustIds.Add(wl.CustId);
            }
            else
            {
                if (wl.RoleId == RoleId.Member)
                {
                    creditCustIds.Add(wl.CustId);
                }
                else
                {
                    creditSMAIds.Add(wl.SettingCustId);
                }
            }
        }

        return winLossService.GetAccumulatedWinLoss(
            creditCustIds.JoinWithComma(),
            creditSMAIds.JoinWithComma(),
            depositCustIds.JoinWithComma(),
            settlementDate);
    }

    private static AccumulatedWinLossParam BuildAccumulatedWinLossParam(
        IEnumerable<TicketDetail> ticketDetails,
        WinLossSetting winLossSetting,
        AccumulatedWinLoss accumulatedWinLoss,
        DateTime settlementDate,
        IDictionary<short, bool> creditBetTypes,
        IDictionary<short, bool> depositBetTypes)
    {
        var accumulatedWinLossType = accumulatedWinLoss == null || winLossSetting.InitSettingTime > accumulatedWinLoss.UpdatedTime
            ? "i" // AccumulatedWinLossType_Insert
            : string.Empty;
        var offeredBetTypes = winLossSetting.IsDeposit ? depositBetTypes : creditBetTypes;

        return new AccumulatedWinLossParam(
            winLossSetting.SettingCustId,
            winLossSetting.IsDeposit,
            SumWinLoss(offeredBetTypes, ticketDetails, winLossSetting),
            settlementDate,
            accumulatedWinLossType,
            winLossSetting.RoleId);
    }

    private static decimal SumWinLoss(
        IDictionary<short, bool> offeredBetTypes,
        IEnumerable<TicketDetail> ticketDetails,
        WinLossSetting winLossSetting)
    {
        var validTicketDetails = ticketDetails
            .Where(t => t.SettlementTime >= winLossSetting.InitSettingTime && offeredBetTypes.ContainsKey(t.BetType));

        return winLossSetting.RoleId switch
        {
            RoleId.Member => validTicketDetails.Sum(t => t.WinLoss - t.ActualStake),
            RoleId.Agent => validTicketDetails.Sum(t => t.AgentWinLoss),
            RoleId.Master => validTicketDetails.Sum(t => t.MasterWinLoss),
            RoleId.Super => validTicketDetails.Sum(t => t.SuperWinLoss),
            _ => throw new ArgumentOutOfRangeException(nameof(winLossSetting.RoleId), $"Not expected RoleId value: {winLossSetting.RoleId}"),
        };
    }
}
