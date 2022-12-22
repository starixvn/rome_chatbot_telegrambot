namespace WLMonitored.Shared.Criteria;

public sealed class GetCreditAccumulatedWinLossCriteria : GetBaseAccumulatedWinLossCriteria
{
    public GetCreditAccumulatedWinLossCriteria(
        string custIds,
        string smaIds,
        DateTime currentDate,
        string recommends = null) : base(custIds, currentDate)
    {
        SMAIds = smaIds;
        Recommends = recommends;
    }

    public string Recommends { get; }

    public string SMAIds { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_AccumulatedDataSel";
}