using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.LastBets;

public class GetFiltersCriteria : CriteriaBase
{
    public GetFiltersCriteria(int custId, int subAccId)
    {
        CustId = custId;
        SubAccId = subAccId;
    }

    public int CustId { get; }

    public int SubAccId { get; }

    public override string GetSettingKey()
        => "Age_CustLastBetFilter_Sel";

    public override bool IsValid()
        => CustId > 0 && SubAccId >= 0;
}