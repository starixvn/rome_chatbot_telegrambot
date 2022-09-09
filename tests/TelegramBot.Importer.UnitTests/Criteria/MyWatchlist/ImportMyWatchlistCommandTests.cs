using TelegramBot.Importer.Criteria.MyWatchlist;

namespace TelegramBot.Importer.UnitTests.Criteria.MyWatchlist;

public class ImportMyWatchlistCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string xmlData, string xmlDataLog, string lastDataRow, string serverName, int maxLogId, string environment)
    {
        // Arrange
        var criteria = new ImportMyWatchlistCommand(xmlData, xmlDataLog, lastDataRow, serverName, maxLogId, environment);

        // Act & Assert
        Assert.Equal("Age_Chatbot_MyWatchlistImport", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_XmlDataIsNullOrWhitSpace_ReturnsFalse(string xmlData)
    {
        // Arrange
        var criteria = new ImportMyWatchlistCommand(xmlData, "xmlDataLog", "lastDataRow", "serverName", 1, "environment");

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_XmlDataLogIsNullOrWhitSpace_ReturnsFalse(string xmlDataLog)
    {
        // Arrange
        var criteria = new ImportMyWatchlistCommand("xmlData", xmlDataLog, "lastDataRow", "serverName", 1, "environment");

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_ValidCriteria_ReturnsTrue(string xmlData, string xmlDataLog, string lastDataRow, string serverName, int maxLogId, string environment)
    {
        // Arrange
        var criteria = new ImportMyWatchlistCommand(xmlData, xmlDataLog, lastDataRow, serverName, maxLogId, environment);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}