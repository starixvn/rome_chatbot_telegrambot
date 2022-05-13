using Fanex.Data.Repository;

namespace TelegramBot.SenderService.Criteria;

public class GetCustomerStatusCriteria : CriteriaBase
{
    public GetCustomerStatusCriteria(string custList)
        => CustList = custList;

    public string CustList { get; }

    public override string GetSettingKey() => "Age_Chatbot_CustomerStatusSel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(CustList);
}