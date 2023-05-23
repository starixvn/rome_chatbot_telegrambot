using TelegramBot.Modules.Function.Models;
using TelegramBot.Modules.Setting.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Setting.Criteria;

public class CustBotMessengerSelCriteriaTest
{
    private CustBotMessengerSelCriteria criteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        criteria = new CustBotMessengerSelCriteria(1, 1, true);
        //Act
        var result = criteria.GetSettingKey();

        //Assert
        Assert.Equal("CustBotMessengerSel", result);
    }

    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        criteria = new CustBotMessengerSelCriteria(1, 1, true);

        var result = criteria.IsValid();

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_inValidCase()
    {
        //case CustId < 0
        criteria = new CustBotMessengerSelCriteria(0, 1, true);
        var case1 = criteria.IsValid();
        Assert.False(case1);
    }
}