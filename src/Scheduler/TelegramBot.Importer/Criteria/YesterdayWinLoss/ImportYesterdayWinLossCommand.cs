namespace TelegramBot.Importer.Criteria.YesterdayWinLoss;

public class ImportYesterdayWinLossCommand : NonQueryCommandBase
{
    public ImportYesterdayWinLossCommand(
        string xmlData,
        DateTime winLossDate,
        string serviceName,
        string environment,
        int maxUserId) : base(xmlData, environment)
    {
        WinLostDate = winLossDate;
        ServiceName = serviceName;
        MaxUserId = maxUserId;
    }

    public DateTime WinLostDate { get; }

    public string ServiceName { get; }

    public int MaxUserId { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_YesterdateWinlostImport";
}