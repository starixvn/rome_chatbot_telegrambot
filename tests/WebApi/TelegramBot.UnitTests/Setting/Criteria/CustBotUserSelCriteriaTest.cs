using TelegramBot.Modules.Setting.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Setting.Criteria;

public class CustBotUserSelCriteriaTest
{
    private CustBotUserSelCriteria criteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        criteria = new CustBotUserSelCriteria(1, 1, true);
        //Act
        var result = criteria.GetSettingKey();

        //Assert
        Assert.Equal("CustBotUserSel", result);
    }

    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        criteria = new CustBotUserSelCriteria(1, 1, true);

        var result = criteria.IsValid();

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_inValidCase()
    {
        //case ChatId < 0
        criteria = new CustBotUserSelCriteria(0, 1, true);
        var case1 = criteria.IsValid();
        Assert.False(case1);
    }
}