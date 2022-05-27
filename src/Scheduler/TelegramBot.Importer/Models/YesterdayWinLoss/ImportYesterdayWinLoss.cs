namespace TelegramBot.Importer.Models.PersonalMessages;

public record ImportYesterdayWinLoss(int UserId, string Message, string Status)
{
    public string ToXml()
        => $"<r UserId=\"{UserId}\" Message=\"{Message}\" Status=\"{Status}\"/>";
}