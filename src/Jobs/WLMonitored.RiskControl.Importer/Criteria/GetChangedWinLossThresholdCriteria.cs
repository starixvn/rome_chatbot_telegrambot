using Fanex.Data.Repository;

namespace WLMonitored.RiskControl.Importer.Criteria;

public sealed class GetChangedWinLossThresholdCriteria : CriteriaBase
{
    public GetChangedWinLossThresholdCriteria(string superList, bool isTest, DateTime lastScanDate)
    {
        SuperList = superList;
        IsTest = isTest;
        LastScanDate = lastScanDate;
    }

    public byte SportType { get; }

    public bool IsDeposit { get; }

    public string SuperList { get; }

    public bool IsTest { get; }

    public DateTime LastScanDate { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_GetChangedSettingList";

    public override bool IsValid()
        => true;
}