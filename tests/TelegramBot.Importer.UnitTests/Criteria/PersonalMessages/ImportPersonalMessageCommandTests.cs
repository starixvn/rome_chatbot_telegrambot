using TelegramBot.Importer.Criteria.PersonalMessages;

namespace TelegramBot.Importer.UnitTests.Criteria.PersonalMessages;

public class ImportPersonalMessageCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string xmlData, string environment, int maxMsgTargetId)
    {
        // Arrange
        var criteria = new ImportPersonalMessageCommand(xmlData, environment, maxMsgTargetId);

        // Act & Assert
        Assert.Equal("Age_Chatbot_PersonalMessageImport", criteria.GetSettingKey());
    }
}