using TelegramBot.Importer.Criteria.PersonalMessages;

namespace TelegramBot.Importer.UnitTests.Criteria.PersonalMessages;

public class GetPersonalMessageCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(int msgTargetId, int batchSize, bool isTest)
    {
        // Arrange
        var criteria = new GetPersonalMessageCommand(msgTargetId, batchSize, isTest);

        // Act & Assert
        Assert.Equal("Age_Chatbot_PersonalMessage_Sel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void IsValid_InvalidBatchSize_ReturnsFalse(int batchSize)
    {
        // Arrange
        var criteria = new GetPersonalMessageCommand(1, batchSize, true);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_InvalidBatchSize_ReturnsTrue(int msgTargetId, int batchSize, bool isTest)
    {
        // Arrange
        var criteria = new GetPersonalMessageCommand(msgTargetId, batchSize, isTest);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}