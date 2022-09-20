using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria;

public class GetOfferedProductCriteria : CriteriaBase
{
    public GetOfferedProductCriteria(int custId)
    {
        CustId = custId;
    }

    public int CustId { get; }

    public override string GetSettingKey() => "Age_GetOfferedProductByCust";

    public override bool IsValid() => CustId > 0;
}