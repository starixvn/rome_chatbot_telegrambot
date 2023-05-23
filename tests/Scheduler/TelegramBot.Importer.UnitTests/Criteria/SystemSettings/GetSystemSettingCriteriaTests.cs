using TelegramBot.Importer.Criteria.SystemSettings;

namespace TelegramBot.Importer.UnitTests.Criteria.SystemSettings;

public class GetSystemSettingCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string serviceName, string environment)
    {
        // Arrange
        var criteria = new GetSystemSettingCriteria(serviceName, environment);

        // Act & Assert
        Assert.Equal("Age_Chatbot_BotSystemSetting_Sel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_InvalidServiceName_ReturnsFalse(string serviceName)
    {
        // Arrange
        var criteria = new GetSystemSettingCriteria(serviceName, "environment");

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_ValidCriteria_ReturnsTrue(string serviceName, string environment)
    {
        // Arrange
        var criteria = new GetSystemSettingCriteria(serviceName, environment);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}