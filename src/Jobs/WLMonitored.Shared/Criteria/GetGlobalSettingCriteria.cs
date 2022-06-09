using Fanex.Data.Repository;

namespace WLMonitored.Shared.Criteria;

public sealed class GetGlobalSettingCriteria : CriteriaBase
{
    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_GlobalSettingSel";

    public override bool IsValid()
        => true;
}