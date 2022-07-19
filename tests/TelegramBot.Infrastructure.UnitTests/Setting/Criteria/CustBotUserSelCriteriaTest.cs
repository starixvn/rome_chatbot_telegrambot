using TelegramBot.Infrastructure.Setting.Criteria;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Setting.Criteria;

public class CustBotUserSelCriteriaTest
{
    private  CustBotUserSelCriteria criteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        criteria = new CustBotUserSelCriteria(1,1);
        //Act
        var result = criteria.GetSettingKey();
            
        //Assert
        Assert.Equal("CustBotUserSel",result);
    } 
    
    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        criteria = new CustBotUserSelCriteria(1,1);

        var result = criteria.IsValid();
            
        //Assert
        Assert.True(result);
    } 
    
    [Fact]
    public void IsValid_inValidCase()
    {
        //case ChatId < 0
        criteria = new CustBotUserSelCriteria(0,1);
        var case1 = criteria.IsValid();
        Assert.False(case1);
        
        //case BotId < 0
        criteria = new CustBotUserSelCriteria(1,0);
        var case2 = criteria.IsValid();
        Assert.False(case2);
        
    }
}