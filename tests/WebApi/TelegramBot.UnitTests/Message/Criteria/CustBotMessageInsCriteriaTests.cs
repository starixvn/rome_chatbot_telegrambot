using AutoFixture.Xunit2;
using TelegramBot.Modules.Message.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Message.Criteria;

public class CustBotMessageInsCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectValue(int sender, int receiver, long chatId, int botId, short functionId, string message, byte status)
    {
        // Arrange
        var criteria = new CustBotMessageInsCriteria(sender, receiver, chatId, botId, functionId, message, status);

        //Act
        var result = criteria.GetSettingKey();

        //Assert
        Assert.Equal("CustBotMessageIns", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_MessageIsNullOrEmpty_ReturnsFalse(string message)
    {
        // Arrange
        var criteria = new CustBotMessageInsCriteria(1, 2, 3, 4, 5, message, 1);

        //Act
        var result = criteria.IsValid();

        //Assert
        Assert.False(result);
    }

    [Theory, AutoData]
    public void IsValid_ValidMessage_ReturnsFalse(int sender, int receiver, long chatId, int botId, short functionId, string message, byte status)
    {
        // Arrange
        var criteria = new CustBotMessageInsCriteria(sender, receiver, chatId, botId, functionId, message, status);

        //Act
        var result = criteria.IsValid();

        //Assert
        Assert.True(result);
    }
}