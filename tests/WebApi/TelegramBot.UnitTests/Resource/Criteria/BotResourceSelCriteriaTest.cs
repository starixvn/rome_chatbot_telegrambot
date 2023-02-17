using TelegramBot.Modules.Resource.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Resource.Criteria;

public class BotResourceSelCriteriaTest
{

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        // Arrange
        var criteria = new BotResourceSelCriteria("name", "key");

        //Act
        var result = criteria.GetSettingKey();

        //Assert
        Assert.Equal("BotResourceSel", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_InvalidResourceName_ReturnFalse(string resourceName)
    {
        // Act
        var criteria = new BotResourceSelCriteria(resourceName, "key");

        // Assert
        Assert.False(criteria.IsValid());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_InvalidResourceKey_ReturnFalse(string resourceKey)
    {
        // Act
        var criteria = new BotResourceSelCriteria("name", resourceKey);

        // Assert
        Assert.False(criteria.IsValid());
    }

    [Fact]
    public void IsValid_ValidCriteria_ReturnTrue()
    {
        // Act
        var criteria = new BotResourceSelCriteria("name", "key");

        // Assert
        Assert.True(criteria.IsValid());
    }
}