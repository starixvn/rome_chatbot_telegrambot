using TelegramBot.Modules.Setting.Criteria;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Setting.Criteria;

public class CustBotAccountInsCriteriaTest
{
    private CustBotAccountInsCriteria botFunctionSelAllCriteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        botFunctionSelAllCriteria = new CustBotAccountInsCriteria(1, 2, 3, 4, "EN", "192.158.19");
        //Act
        var result = botFunctionSelAllCriteria.GetSettingKey();

        //Assert
        Assert.Equal("CustBotAccountIns", result);
    }

    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        botFunctionSelAllCriteria = new CustBotAccountInsCriteria(1, 2, 3, 4, "EN", "192.158.19");

        var result = botFunctionSelAllCriteria.IsValid();

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_inValidCase()
    {
        //case BotId <0
        botFunctionSelAllCriteria = new CustBotAccountInsCriteria(0, 2, 3, 4, "EN", "192.158.19");
        var case1 = botFunctionSelAllCriteria.IsValid();
        Assert.False(case1);

        //case ChatId  =0
        botFunctionSelAllCriteria = new CustBotAccountInsCriteria(1, 0, 3, 4, "EN", "192.158.19");
        var case2 = botFunctionSelAllCriteria.IsValid();
        Assert.False(case2);

        //case CustId   <0
        botFunctionSelAllCriteria = new CustBotAccountInsCriteria(1, 0, -1, 4, "EN", "192.158.19");
        var case3 = botFunctionSelAllCriteria.IsValid();
        Assert.False(case3);
    }
}