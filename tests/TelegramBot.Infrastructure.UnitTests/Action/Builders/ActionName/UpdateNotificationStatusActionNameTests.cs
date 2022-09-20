using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Infrastructure.Action.Builders.ActionName;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.Models;
using TelegramBot.Infrastructure.Setting.Models;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action.Builders.ActionName;

public class UpdateNotificationStatusActionNameTests
{
    private readonly IResourceService resourceService;
    private readonly UpdateNotificationStatusActionName updateNotificationStatusActionName;

    public UpdateNotificationStatusActionNameTests()
    {
        resourceService = Substitute.For<IResourceService>();
        updateNotificationStatusActionName = new UpdateNotificationStatusActionName(resourceService);
    }

    [Theory, AutoData]
    public async Task BuildActionName_FunctionIsEnabled_ReturnsEnabledName(CustBotAccount account)
    {
        // Arrange
        var action = new BotAction() { ActionId = ActionIds.YesterdayWinLossNotification, Command = FunctionIds.YesterdayWinLossNotification.ToString() };
        account.Functions = new List<BotFunction>{
            new BotFunction{FunctionId = FunctionIds.YesterdayWinLossNotification,Enabled=true},
        };
        resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new BotResource("ActionName", ActionIds.YesterdayWinLossNotification.ToString(), "en-US", "Yesterday WinLoss Notification")));

        // Act
        var actualResult = await updateNotificationStatusActionName.BuildActionName(action, account);

        // Assert
        Assert.Equal("\uD83D\uDD14Yesterday WinLoss Notification", actualResult);
    }

    [Theory, AutoData]
    public async Task BuildActionName_FunctionIsDisabled_ReturnsDisabledName(CustBotAccount account)
    {
        // Arrange
        var action = new BotAction() { ActionId = ActionIds.YesterdayWinLossNotification, Command = FunctionIds.YesterdayWinLossNotification.ToString() };
        account.Functions = new List<BotFunction>{
            new BotFunction{FunctionId = FunctionIds.YesterdayWinLossNotification,Enabled=false},
        };
        resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new BotResource("ActionName", ActionIds.YesterdayWinLossNotification.ToString(), "en-US", "Yesterday WinLoss Notification")));

        // Act
        var actualResult = await updateNotificationStatusActionName.BuildActionName(action, account);

        // Assert
        Assert.Equal("\uD83D\uDD15Yesterday WinLoss Notification", actualResult);
    }
}