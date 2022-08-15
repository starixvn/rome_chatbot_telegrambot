using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria;

public class GetCustomerInfoCriteria : CriteriaBase
{
    public GetCustomerInfoCriteria(int custId)
    {
        CustId = custId;
    }

    public int CustId { get; }

    public override string GetSettingKey() => "Age_GetCustomerInfo";

    public override bool IsValid() => CustId > 0;
}