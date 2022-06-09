namespace WLMonitored.Shared.Criteria;

public sealed class GetCreditAccumulatedWinLossCriteria : GetBaseAccumulatedWinLossCriteria
{
    public GetCreditAccumulatedWinLossCriteria(string custIds, DateTime currentDate)
        : base(custIds, currentDate)
    {
    }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_AccumulatedDataSel";
}