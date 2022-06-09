using Fanex.Data.Repository;

namespace WLMonitored.RiskControl.Importer.Criteria;

public sealed class GetWinLossSettingCriteria : CriteriaBase
{
    public GetWinLossSettingCriteria(string custIds, string superList, bool isTest)
    {
        CustList = custIds;
        SuperList = superList;
        IsTest = isTest;
    }

    public string CustList { get; }

    public byte SportType { get; }

    public bool IsDeposit { get; }

    public string SuperList { get; }

    public bool IsTest { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_GetSettingByCust";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(CustList);
}