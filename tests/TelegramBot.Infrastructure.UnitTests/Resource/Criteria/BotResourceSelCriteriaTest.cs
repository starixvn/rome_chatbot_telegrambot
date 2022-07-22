using TelegramBot.Infrastructure.Resource.Criteria;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Resource.Criteria;

public class BotResourceSelCriteriaTest
{
    private readonly BotResourceSelCriteria botResourceSelCriteria;

    public BotResourceSelCriteriaTest()
    {
        botResourceSelCriteria = new BotResourceSelCriteria("name", "key");
    }

    [Fact]
    public void GetSettingKey_Always_ReturnName()
    {
        //Act
        var result = botResourceSelCriteria.GetSettingKey();

        //Assert
        Assert.Equal("BotResourceSel", result);
    }
}