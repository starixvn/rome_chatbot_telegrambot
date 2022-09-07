using TelegramBot.SenderService.Criteria;

namespace TelegramBot.SenderService.UnitTests.Criteria;

public class GetNotificationCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string environment)
    {
        // Arrange
        var criteria = new GetNotificationCriteria(environment);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_SendNotification", actualResult);
    }

    [Theory, AutoData]
    public void IsValid_Always_ReturnsTrue(string environment)
    {
        // Arrange
        var criteria = new GetNotificationCriteria(environment);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}