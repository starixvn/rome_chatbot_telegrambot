using TelegramBot.Importer.Criteria.SystemSettings;

namespace TelegramBot.Importer.UnitTests.Criteria.SystemSettings;

public class UpdateSystemSettingCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string serviceName, string environment, string setting)
    {
        // Arrange
        var criteria = new UpdateSystemSettingCommand(serviceName, environment, setting);

        // Act & Assert
        Assert.Equal("Age_Chatbot_BotSystemSetting_Upd", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_InvalidServiceName_ReturnsFalse(string serviceName)
    {
        // Arrange
        var criteria = new UpdateSystemSettingCommand(serviceName, "environment", "setting");

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_InvalidSetting_ReturnsFalse(string setting)
    {
        // Arrange
        var criteria = new UpdateSystemSettingCommand("serviceName", "environment", setting);

        // Act & Assert
        Assert.False(criteria.IsValid());
    }

    [Theory, AutoData]
    public void IsValid_ValidCriteria_ReturnsTrue(string serviceName, string environment, string setting)
    {
        // Arrange
        var criteria = new UpdateSystemSettingCommand(serviceName, environment, setting);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}