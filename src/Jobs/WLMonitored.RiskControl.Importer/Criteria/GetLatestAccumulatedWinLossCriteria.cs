using Fanex.Data.Repository;

namespace WLMonitored.RiskControl.Importer.Criteria;

public sealed class GetLatestAccumulatedWinLossCriteria : CriteriaBase
{
    public GetLatestAccumulatedWinLossCriteria(DateTime lastCheckTime, int batchSize)
    {
        LastCheckTime = lastCheckTime;
        BatchNo = batchSize;
    }

    public DateTime LastCheckTime { get; }

    public int BatchNo { get; }


    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_LatestAccumulatedDataSel";

    public override bool IsValid()
        => BatchNo > 0;
}