using NSubstitute;
using TelegramBot.Modules.Function.Models;
using TelegramBot.Modules.Setting.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Setting.Criteria;

public class CustBotFunctionUpdCriteriaTest
{
    private CustBotFunctionUpdCriteria criteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        criteria = new CustBotFunctionUpdCriteria(1, 1, new BotFunction(), 1, "192.168.1.1");
        //Act
        var result = criteria.GetSettingKey();

        //Assert
        Assert.Equal("CustBotFunctionUpd", result);
    }

    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        criteria = new CustBotFunctionUpdCriteria(1, 1, new BotFunction(), 1, "192.168.1.1");

        var result = criteria.IsValid();

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_inValidCase()
    {
        //case UserId < 0
        criteria = new CustBotFunctionUpdCriteria(0, 1, new BotFunction(), 1, "192.168.1.1");
        var case1 = criteria.IsValid();
        Assert.False(case1);
    }
}