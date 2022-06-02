namespace TelegramBot.Importer.Criteria.YesterdayWinLoss;

public class ImportYesterdayWinLossCommand : NonQueryCommandBase
{
    public ImportYesterdayWinLossCommand(
        string xmlData,
        DateTime winLossDate,
        string environment,
        int maxUserId) : base(xmlData, environment)
    {
        WinLostDate = winLossDate;
        MaxUserId = maxUserId;
    }

    public DateTime WinLostDate { get; }

    public int MaxUserId { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_YesterdateWinlostImport";
}