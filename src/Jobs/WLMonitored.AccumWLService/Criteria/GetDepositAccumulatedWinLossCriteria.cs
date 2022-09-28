using WLMonitored.Shared.Criteria;

namespace WLMonitored.AccumWLService.Criteria;

public sealed class GetDepositAccumulatedWinLossCriteria : GetBaseAccumulatedWinLossCriteria
{
    public GetDepositAccumulatedWinLossCriteria(string custIds, DateTime currentDate)
        : base(custIds, currentDate)
    {
    }

    public override string GetSettingKey()
        => "Acc_Dep_WLMonitored_Service_AccumulatedDataSel";
}