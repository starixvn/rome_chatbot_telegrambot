using TelegramBot.Infrastructure.Setting.Criteria;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Setting.Criteria;

public class CustBotAccountStatusUpdCriteriaTest
{
    private  CustBotAccountStatusUpdCriteria criteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        criteria = new CustBotAccountStatusUpdCriteria(1,2,3,true,true,"192.158.19");
        //Act
        var result = criteria.GetSettingKey();
            
        //Assert
        Assert.Equal("CustBotAccountStatusUpd",result);
    } 
    
    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        criteria = new CustBotAccountStatusUpdCriteria(1,2,3,true,true,"192.158.19");

        var result = criteria.IsValid();
            
        //Assert
        Assert.True(result);
    } 
    
    [Fact]
    public void IsValid_inValidCase()
    {
      
        //case BotId <0
        criteria = new CustBotAccountStatusUpdCriteria(0,2,3,true,true,"192.158.19");
        var case1 = criteria.IsValid();
        Assert.False(case1);
        
        
        //case CustId <0
        criteria = new CustBotAccountStatusUpdCriteria(1,0,-1,true,true,"192.158.19");
        var case2 = criteria.IsValid();
        Assert.False(case2);
    }
}