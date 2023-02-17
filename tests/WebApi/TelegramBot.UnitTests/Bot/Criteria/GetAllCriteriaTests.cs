using AutoFixture.Xunit2;
using TelegramBot.Modules.Bot.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Bot.Criteria;

public class GetAllCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        // Act
        var criteria = new GetAllCriteria(true);

        //Act
        var result = criteria.GetSettingKey();

        //Assert
        Assert.Equal("BotSelAll", result);
    }

    [Fact]
    public void IsValid_Always_ReturnTrue()
    {
        // Act
        var criteria = new GetAllCriteria(true);

        //Act
        var result = criteria.IsValid();

        //Assert
        Assert.True(result);
    }


    [Theory, AutoData]
    public void IsTest_Always_ReturnCorrectValue(bool isTest)
    {
        // Act
        var criteria = new GetAllCriteria(isTest);

        //Assert
        Assert.Equal(isTest, criteria.IsTest);
    }
}