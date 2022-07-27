using System.Collections.Generic;
using System.Threading.Tasks;
using Alpha.API.Client.Services;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.WebApi.Filters;
using TelegramBot.WebApi.Params;
using Xunit;

namespace TelegramBot.WebApi.UnitTests.Filters;

public class AccountStatusFilterAttributeTests
{
    private readonly ICustomerService customerService;
    private readonly ISettingService settingService;
    private readonly IBotService botService;
    private readonly IMessageBuilder messageBuilder;
    private readonly IMessageSender messageSender;
    private readonly Fanex.Logging.ILogger logger;
    private readonly AccountStatusFilterAttribute accountStatusFilterAttribute;
    private readonly Controller controller;
    private readonly IList<IFilterMetadata> metadata;

    public AccountStatusFilterAttributeTests()
    {
        customerService = Substitute.For<ICustomerService>();
        settingService = Substitute.For<ISettingService>();
        botService = Substitute.For<IBotService>();
        messageBuilder = Substitute.For<IMessageBuilder>();
        messageSender = Substitute.For<IMessageSender>();
        logger = Substitute.For<Fanex.Logging.ILogger>();
        controller = Substitute.For<Controller>();
        metadata = new List<IFilterMetadata>();
        accountStatusFilterAttribute = new AccountStatusFilterAttribute(customerService, settingService, botService, messageBuilder, messageSender, logger);
    }

    [Theory, AutoData]
    public async Task OnActionExecutionAsync_HasParamArguments_CallsGetBot(CommandParam param, BotEntity bot)
    {
        // Arrange
        var context = StubActionExecutingContext(new Dictionary<string, object> { { "param", param } });
        var next = StubActionExecutionDelegate(context);
        botService.GetAsync(param.MessengerBotId).Returns(Task.FromResult(bot));

        // Act
        await accountStatusFilterAttribute.OnActionExecutionAsync(context, next);

        // Assert
        await botService.Received().GetAsync(param.MessengerBotId);
    }

    [Theory, AutoData]
    public async Task OnActionExecutionAsync_CusBotAccountNotFound_NotCallsGetCustomerStatus(CommandParam param, BotEntity bot)
    {
        // Arrange
        var context = StubActionExecutingContext(new Dictionary<string, object> { { "param", param } });
        var next = StubActionExecutionDelegate(context);
        botService.GetAsync(param.MessengerBotId).Returns(Task.FromResult(bot));
        CustBotAccount account = null;
        settingService.GetCustBotAccount(param.ChatId, bot.BotId).Returns(Task.FromResult(account));

        // Act
        await accountStatusFilterAttribute.OnActionExecutionAsync(context, next);

        // Assert
        await customerService.DidNotReceive().GetCustomerStatus(Arg.Any<int>(), Arg.Any<int>());
    }

    [Theory, AutoData]
    public async Task OnActionExecutionAsync_CusBotAccountFounded_CallsGetCustomerStatus(CommandParam param, BotEntity bot, CustBotAccount account)
    {
        // Arrange
        var context = StubActionExecutingContext(new Dictionary<string, object> { { "param", param } });
        var next = StubActionExecutionDelegate(context);
        botService.GetAsync(param.MessengerBotId).Returns(Task.FromResult(bot));
        settingService.GetCustBotAccount(param.ChatId, bot.BotId).Returns(Task.FromResult(account));

        // Act
        await accountStatusFilterAttribute.OnActionExecutionAsync(context, next);

        // Assert
        await customerService.Received().GetCustomerStatus(account.CustId, account.SubAccId);
    }

    private ActionExecutingContext StubActionExecutingContext(Dictionary<string, object> actionArguments)
        => new(
            new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor(),
            },
            metadata,
            actionArguments,
            controller);

    private ActionExecutionDelegate StubActionExecutionDelegate(ActionExecutingContext context)
        => () =>
            {
                var ctx = new ActionExecutedContext(context, metadata, controller);
                return Task.FromResult(ctx);
            };
}