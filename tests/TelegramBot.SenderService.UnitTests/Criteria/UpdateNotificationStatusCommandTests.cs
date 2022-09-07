using TelegramBot.SenderService.Criteria;

namespace TelegramBot.SenderService.UnitTests.Criteria;

public class UpdateNotificationStatusCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string xmlData, string xmlLastNotification, string environment)
    {
        // Arrange
        var criteria = new UpdateNotificationStatusCommand(xmlData, xmlLastNotification, environment);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_SendNotification_Status_Upd", actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_XmlDataIsNullOrWhiteSpace_ReturnsFalse(string xmlData)
    {
        // Arrange
        var criteria = new UpdateNotificationStatusCommand(xmlData, "XmlLastNotification ", "UAT");

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory, AutoData]
    public void IsValid_XmlDataIsValid_ReturnsFalse(string xmlData)
    {
        // Arrange
        var criteria = new UpdateNotificationStatusCommand(xmlData, "XmlLastNotification ", "UAT");

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_XmlLastNotificationIsNullOrWhiteSpace_ReturnsFalse(string xmlLastNotification)
    {
        // Arrange
        var criteria = new UpdateNotificationStatusCommand("xmlData", xmlLastNotification, "UAT");

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory, AutoData]
    public void IsValid_XmlLastNotificationIsValid_ReturnsFalse(string xmlLastNotification)
    {
        // Arrange
        var criteria = new UpdateNotificationStatusCommand("xmlData", xmlLastNotification, "UAT");

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}