using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.YesterdayWinLoss;

public class ImportYesterdayWinLossCommand : NonQueryCommand
{
    public ImportYesterdayWinLossCommand(
        string xmlData,
        DateTime winLossDate,
        string environment,
        int maxUserId)
    {
        XmlData = xmlData;
        WinLostDate = winLossDate;
        Environment = environment;
        MaxUserId = maxUserId;
    }

    public string XmlData { get; }

    public DateTime WinLostDate { get; }

    public string Environment { get; }

    public int MaxUserId { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = default!;

    public override string GetSettingKey()
        => "Age_Chatbot_YesterdateWinlostImport";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}