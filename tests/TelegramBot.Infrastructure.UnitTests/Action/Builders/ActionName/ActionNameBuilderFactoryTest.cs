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

    [Theory]
    [InlineData(ActionIds.YesterdayWinLossNotification)]
    [InlineData(ActionIds.PersonalMessageNotification)]
    [InlineData(ActionIds.ProblemAccountNotification)]
    [InlineData(ActionIds.MemberWinLossLimitNotification)]
    [InlineData(ActionIds.ChangePasswordAlert)]
    [InlineData(ActionIds.ChangeSecurityCodeAlert)]
    public void GetBuilder_UpdateNotificationStatusAction_ReturnUpdateNotificationStatusActionName(short actionId)
    {
        // Act
        var builder = actionNameBuilderFactory.GetBuilder(actionId);

        // Assert
        Assert.Equal(typeof(UpdateNotificationStatusActionName), builder.GetType());
    }

    [Theory]
    [InlineData(ActionIds.English)]
    [InlineData(ActionIds.ChineseTraditional)]
    [InlineData(ActionIds.ChineseSimplified)]
    [InlineData(ActionIds.Thai)]
    [InlineData(ActionIds.Vietnamese)]
    public void GetBuilder_UpdateLanguageAction_ReturnUpdateLanguageActionName(short actionId)
    {
        // Act
        var builder = actionNameBuilderFactory.GetBuilder(actionId);

        // Assert
        Assert.Equal(typeof(UpdateLanguageActionName), builder.GetType());
    }


    [Fact]
    public void GetBuilder_OtherAction_ReturnUpdateLanguageActionName()
    {
        // Act
        var builder = actionNameBuilderFactory.GetBuilder(ActionIds.Settings);

        // Assert
        Assert.Equal(typeof(DefaultActionNameBuilder), builder.GetType());
    }
}