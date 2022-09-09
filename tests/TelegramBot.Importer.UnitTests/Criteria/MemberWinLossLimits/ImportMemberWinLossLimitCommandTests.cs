using TelegramBot.Importer.Criteria.MemberWinLossLimits;

namespace TelegramBot.Importer.UnitTests.Criteria.MemberWinLossLimits;

public class ImportMemberWinLossLimitCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string xmlData, string xmlDataLog, long maxLogId, string environment)
    {
        // Arrange
        var criteria = new ImportMemberWinLossLimitCommand(xmlData, xmlDataLog, environment, maxLogId);

        // Act & Assert
        Assert.Equal("Age_Chatbot_WinLossLimitNotificationImport", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_XmlDataIsNullOrWhitSpace_ReturnsFalse(string xmlData)
    {
        // Arrange
        var criteria = new ImportMemberWinLossLimitCommand(xmlData, "xmlDataLog", "environment", 1);

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
        var criteria = new ImportMemberWinLossLimitCommand("xmlData", xmlDataLog, "environment", 1);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_ValidCriteria_ReturnsTrue(string xmlData, string xmlDataLog, int maxLogId, string environment)
    {
        // Arrange
        var criteria = new ImportMemberWinLossLimitCommand(xmlData, xmlDataLog, environment, maxLogId);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}