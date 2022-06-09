using Fanex.Data.Repository;

namespace WLMonitored.Shared.Criteria;

public class GetBaseAccumulatedWinLossCriteria : CriteriaBase
{
    protected GetBaseAccumulatedWinLossCriteria(string custIds, DateTime currentDate)
    {
        CustIds = custIds;
        CurrentDate = currentDate;
    }

    public string CustIds { get; }

    public DateTime CurrentDate { get; }

    public override string GetSettingKey()
        => throw new NotImplementedException();

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(CustIds);
}