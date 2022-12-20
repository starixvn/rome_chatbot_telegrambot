using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Customers;

public class GetYesterdayWinLossLinkedCustomerCommand : NonQueryCommand
{
    public GetYesterdayWinLossLinkedCustomerCommand(
        DateTime winLostDate,
        string serviceName,
        string environment)
    {
        WinLostDate = winLostDate;
        ServiceName = serviceName;
        Environment = environment;
    }

    public DateTime WinLostDate { get; }

    public string ServiceName { get; }

    public string Environment { get; }

    public int MaxUserId { get; set; }

    public override string GetSettingKey()
        => "Age_Chatbot_YesterdateWinlost_LinkedAccounts_Sel";

    public override bool IsValid()
        => true;
}