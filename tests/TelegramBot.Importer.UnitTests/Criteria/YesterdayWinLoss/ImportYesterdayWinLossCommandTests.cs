using TelegramBot.Importer.Criteria.YesterdayWinLoss;

namespace TelegramBot.Importer.UnitTests.Criteria.YesterdayWinLoss;

public class ImportYesterdayWinLossCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string xmlData, DateTime winLossDate, string environment, int maxUserId)
    {
        // Arrange
        var criteria = new ImportYesterdayWinLossCommand(xmlData, winLossDate, environment, maxUserId);

        // Act & Assert
        Assert.Equal("Age_Chatbot_YesterdateWinlostImport", criteria.GetSettingKey());
    }
}