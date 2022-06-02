using TelegramBot.Infrastructure.Setting.Criteria;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Setting.Criteria;

public class CustBotAccountLanguageUpdCriteriaTest
{
    private  CustBotAccountLanguageUpdCriteria criteria;

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        criteria = new CustBotAccountLanguageUpdCriteria(1,"EN","192.158.1.1");
        //Act
        var result = criteria.GetSettingKey();
            
        //Assert
        Assert.Equal("CustBotAccountLanguageUpd",result);
    } 
    
    [Fact]
    public void IsValid_ValidCase()
    {
        //Act
        criteria = new CustBotAccountLanguageUpdCriteria(1,"EN","192.158.1.1");

        var result = criteria.IsValid();
            
        //Assert
        Assert.Equal(true,result);
    } 
    
    [Fact]
    public void IsValid_inValidCase()
    {
        //case UserId <0
        criteria = new CustBotAccountLanguageUpdCriteria(0,"EN","192.158.1.1");
        var case1 = criteria.IsValid();
        Assert.Equal(false,case1);
        
        //case NullOrWhiteSpace Language
        criteria = new CustBotAccountLanguageUpdCriteria(1,"","192.158.19");
        var case2 = criteria.IsValid();
        Assert.Equal(false,case2);
    }
}