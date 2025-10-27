using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using TelegramBot.Modules.Bot;
using TelegramBot.Modules.Bot.Models;
using TelegramBot.Modules.Message;
using TelegramBot.Shared.Models;
using TelegramBot.WebApi.Filters;
using TelegramBot.WebApi.Params;
using Xunit;

namespace TelegramBot.WebApi.UnitTests.Filters;

public class AutoReplyUMFilterAttributeTests
{
    private readonly IMaintenanceService maintenanceService;
    private readonly IMessageSender messageSender;
    private readonly IMessageBuilder messageBuilder;
    private readonly IBotService botService;
    private readonly AutoReplyUMFilterAttribute autoReplyUMFilterAttribute;
    private readonly Controller controller;
    private readonly IList<IFilterMetadata> metadata;

    public AutoReplyUMFilterAttributeTests()
    {
        maintenanceService = Substitute.For<IMaintenanceService>();
        messageSender = Substitute.For<IMessageSender>();
        messageBuilder = Substitute.For<IMessageBuilder>();
        botService = Substitute.For<IBotService>();
        autoReplyUMFilterAttribute = new AutoReplyUMFilterAttribute(maintenanceService, messageSender, messageBuilder, botService);
        controller = Substitute.For<Controller>();
        metadata = new List<IFilterMetadata>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_HaveNOTParamArguments_NOTCallsGetUnderMaintenanceTimeAsync()
    {
        // Arrange
        var context = StubActionExecutingContext(new Dictionary<string, object>());
        var next = StubActionExecutionDelegate(context);

        // Act
        await autoReplyUMFilterAttribute.OnActionExecutionAsync(context, next);

        // Assert
        await maintenanceService.DidNotReceive().GetUnderMaintenanceTimeAsync();
    }

    [Theory, AutoData]
    public async Task OnActionExecutionAsync_HaveParamArguments_CallsGetUnderMaintenanceTimeAsync(CommandParam param)
    {
        // Arrange
        var context = StubActionExecutingContext(new Dictionary<string, object> { { "param", param } });
        var next = StubActionExecutionDelegate(context);
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime(false, "", "")));

        // Act
        await autoReplyUMFilterAttribute.OnActionExecutionAsync(context, next);

        // Assert
        await maintenanceService.Received().GetUnderMaintenanceTimeAsync();
    }

    [Theory, AutoData]
    public async Task OnActionExecutionAsync_IsUM_CallsSendUnderMaintenanceMessage(CommandParam param, BotEntity bot)
    {
        // Arrange
        var context = StubActionExecutingContext(new Dictionary<string, object> { { "param", param } });
        var next = StubActionExecutionDelegate(context);
        botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(new UnderMaintenanceTime(true, "", "")));
        messageBuilder.BuildUnderMaintenanceMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("UMMessage");


        // Act
        await autoReplyUMFilterAttribute.OnActionExecutionAsync(context, next);

        // Assert
        await messageSender.Received().SendMessage(Arg.Any<Messenger>(), "UMMessage");
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