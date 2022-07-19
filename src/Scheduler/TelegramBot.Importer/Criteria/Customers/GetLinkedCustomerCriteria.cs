using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Customers;

public class GetLinkedCustomerCriteria : NonQueryCommand
{
    public GetLinkedCustomerCriteria(
        string functionKey,
        string custIdList,
        string environment)
    {
        FunctionKey = functionKey;
        CustIdList = custIdList;
        Environment = environment;
    }

    public string FunctionKey { get; }

    public string CustIdList { get; }

    public string Environment { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_GetLinkedAccounts";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(FunctionKey)
        && !string.IsNullOrWhiteSpace(CustIdList);
}