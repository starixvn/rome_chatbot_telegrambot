namespace WLMonitored.AccumWLService.Models;

public record WinLossSetting(
    int CustId,
    DateTime InitSettingTime,
    bool IsDeposit,
    int SRecommend
);