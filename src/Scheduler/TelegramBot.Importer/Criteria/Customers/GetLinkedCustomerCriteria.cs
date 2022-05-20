using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Customers;

public class GetLinkedCustomerCriteria : NonQueryCommand
{
    public GetLinkedCustomerCriteria(
        string system,
        bool botStatus,
        bool isBotTest,
        string functionKey,
        string custIdList,
        string environment)
    {
        System = system;
        BotStatus = botStatus;
        IsBotTest = isBotTest;
        FunctionKey = functionKey;
        CustIdList = custIdList;
        Environment = environment;
    }

    public string System { get; }

    public bool BotStatus { get; }

    public bool IsBotTest { get; }

    public string FunctionKey { get; }

    public string CustIdList { get; }

    public string Environment { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_GetLinkedAccounts";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(System)
        && !string.IsNullOrWhiteSpace(FunctionKey)
        && !string.IsNullOrWhiteSpace(CustIdList);
}