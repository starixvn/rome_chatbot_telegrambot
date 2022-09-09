using TelegramBot.Importer.Criteria.MyWatchlist;

namespace TelegramBot.Importer.UnitTests.Criteria.MyWatchlist;

public class GetMyWatchlistCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(int lastLogId, int batchSize, string lastDataRow, string serverName)
    {
        // Arrange
        var criteria = new GetMyWatchlistCriteria(lastLogId, batchSize, lastDataRow, serverName);

        // Act & Assert
        Assert.Equal("Age_Chatbot_MyWatchlist_Sel", criteria.GetSettingKey());
    }

    [Theory, AutoData]
    public void IsValid_Always_ReturnsTrue(int lastLogId, int batchSize, string lastDataRow, string serverName)
    {
        // Arrange
        var criteria = new GetMyWatchlistCriteria(lastLogId, batchSize, lastDataRow, serverName);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}