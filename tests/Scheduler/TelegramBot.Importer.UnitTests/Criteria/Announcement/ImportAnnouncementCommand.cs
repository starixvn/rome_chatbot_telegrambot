using TelegramBot.Importer.Criteria.Announcement;

namespace TelegramBot.Importer.UnitTests.Criteria.Announcement;

public class ImportAnnouncementCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string xmlData, string environment, DateTime lastQueryTime)
    {
        // Arrange
        var criteria = new ImportAnnouncementCommand(xmlData, environment, lastQueryTime);

        // Act & Assert
        Assert.Equal("Age_Chatbot_AnnouncementNotificationImport", criteria.GetSettingKey());
    }
}