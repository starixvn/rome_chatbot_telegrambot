using Fanex.Data.Repository;

namespace WLMonitored.AccumWLService.Criteria;

public sealed class GetBetTypeCriteria : CriteriaBase
{
    public GetBetTypeCriteria(string customerType)
    {
        Model = customerType;
    }

    public string Model { get; }

    public override string GetSettingKey()
        => "Acc_WLMonitored_Service_BettypeOffered";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(Model);
}