using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Customers;

public class GetLinkedCustomerByFunctionCriteria : CriteriaBase
{
    public GetLinkedCustomerByFunctionCriteria(string functionKey, string environment = "")
    {
        FunctionKey = functionKey;
        Environment = environment;
    }

    public string FunctionKey { get; }

    public string Environment { get; }

    public override string GetSettingKey() => "Age_Chatbot_GetAllLinkedAccountsByFunction";

    public override bool IsValid() => !string.IsNullOrEmpty(FunctionKey);
}