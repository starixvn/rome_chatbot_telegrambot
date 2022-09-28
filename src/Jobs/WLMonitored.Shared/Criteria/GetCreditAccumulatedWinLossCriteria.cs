namespace WLMonitored.Shared.Criteria;

public sealed class GetCreditAccumulatedWinLossCriteria : GetBaseAccumulatedWinLossCriteria
{
    public GetCreditAccumulatedWinLossCriteria(string custIds, DateTime currentDate, string recommends = null)
        : base(custIds, currentDate)
    {
        Recommends = recommends;
    }

    public string Recommends { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_AccumulatedDataSel";
}