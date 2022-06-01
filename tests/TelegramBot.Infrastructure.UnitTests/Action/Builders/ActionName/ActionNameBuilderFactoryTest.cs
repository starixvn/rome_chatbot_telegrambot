using NSubstitute;
using TelegramBot.Infrastructure.Action.Builders.ActionName;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Resource;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action.Builders.ActionName;

public class ActionNameBuilderFactoryTest
{
    private readonly IResourceService resourceService;
    private readonly ActionNameBuilderFactory actionNameBuilderFactory;

    public ActionNameBuilderFactoryTest()
    {
        resourceService = Substitute.For<IResourceService>();
        actionNameBuilderFactory = new ActionNameBuilderFactory(resourceService);
    }

    [Fact]
    public void GetBuilder_ActionId_ReturnUpdateNotificationStatusActionName()
    {
        var builder1 = actionNameBuilderFactory.GetBuilder(ActionIds.YesterdayWinLossNotification);
        var builder2 = actionNameBuilderFactory.GetBuilder(ActionIds.PersonalMessageNotification);
        var builder3 = actionNameBuilderFactory.GetBuilder(ActionIds.ProblemAccountNotification);
        var builder4 = actionNameBuilderFactory.GetBuilder(ActionIds.MemberWinLossLimitNotification);
        var builder5 = actionNameBuilderFactory.GetBuilder(ActionIds.ChangePasswordAlert);
        var builder6 = actionNameBuilderFactory.GetBuilder(ActionIds.ChangeSecurityCodeAlert);
        
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder1.GetType());
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder2.GetType());
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder3.GetType());
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder4.GetType());
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder5.GetType());
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder6.GetType());
    }  
    
    [Fact]
    public void GetBuilder_ActionId_ReturnUpdateLanguageActionName()
    {
        var builder1 = actionNameBuilderFactory.GetBuilder(ActionIds.English);
        var builder2 = actionNameBuilderFactory.GetBuilder(ActionIds.ChineseTraditional);
        var builder3 = actionNameBuilderFactory.GetBuilder(ActionIds.ChineseSimplified);
        var builder4 = actionNameBuilderFactory.GetBuilder(ActionIds.Thai);
        var builder5 = actionNameBuilderFactory.GetBuilder(ActionIds.Vietnamese);
        
        Assert.Equal(typeof(UpdateLanguageActionName), builder1.GetType());
        Assert.Equal(typeof(UpdateLanguageActionName), builder2.GetType());
        Assert.Equal(typeof(UpdateLanguageActionName), builder3.GetType());
        Assert.Equal(typeof(UpdateLanguageActionName), builder4.GetType());
        Assert.Equal(typeof(UpdateLanguageActionName), builder5.GetType());
    }  
    
    [Fact]
    public void GetBuilder_DefaultActionId_ReturnDefaultActionNameBuilder()
    {
        var builder1 = actionNameBuilderFactory.GetBuilder(99);
        var builder2 = actionNameBuilderFactory.GetBuilder(99);
        
        Assert.Equal(typeof(DefaultActionNameBuilder), builder1.GetType());
        Assert.Equal(builder1, builder2);
    }
}