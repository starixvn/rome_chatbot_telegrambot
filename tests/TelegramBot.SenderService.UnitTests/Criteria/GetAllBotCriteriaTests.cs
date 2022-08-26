using TelegramBot.SenderService.Criteria;

namespace TelegramBot.SenderService.UnitTests.Criteria;

public class GetAllBotCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(bool isTest)
    {
        // Arrange
        var criteria = new GetAllBotCriteria(isTest);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("BotSelAll", actualResult);
    }

    [Theory, AutoData]
    public void IsValid_Always_ReturnsTrue(bool isTest)
    {
        // Arrange
        var criteria = new GetAllBotCriteria(isTest);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}