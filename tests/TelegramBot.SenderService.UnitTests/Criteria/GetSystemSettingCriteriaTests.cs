using TelegramBot.SenderService.Criteria;

namespace TelegramBot.SenderService.UnitTests.Criteria;

public class GetSystemSettingCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string serviceName, string environment)
    {
        // Arrange
        var criteria = new GetSystemSettingCriteria(serviceName, environment);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_BotSystemSetting_Sel", actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_ServiceNameIsNullOrWhiteSpace_ReturnsFalse(string serviceName)
    {
        // Arrange
        var criteria = new GetSystemSettingCriteria(serviceName, "UAT");

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory, AutoData]
    public void IsValid_ServiceNameIsValid_ReturnsFalse(string serviceName, string environment)
    {
        // Arrange
        var criteria = new GetSystemSettingCriteria(serviceName, environment);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}