namespace WLMonitored.AccumWLService.Models;

public record TicketDetail(
    long Id,
    long TransId,
    int CustId,
    short BetType,
    decimal ActualStake,
    decimal WinLoss,
    DateTime SettlementTime
);