using System.Threading.Tasks;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Modules.Bot;
using TelegramBot.Modules.Bot.Models;
using TelegramBot.Modules.Command;
using TelegramBot.Modules.Command.Models;
using TelegramBot.WebApi.Controllers;
using TelegramBot.WebApi.Params;
using Xunit;

namespace TelegramBot.WebApi.UnitTests;

[Trait("Controllers", "Command")]
public class CommandControllerTests
{
    private readonly IBotService botService;
    private readonly ICommandService commandService;
    private readonly CommandController commandController;

    public CommandControllerTests()
    {
        botService = Substitute.For<IBotService>();
        commandService = Substitute.For<ICommandService>();
        commandController = new CommandController(botService, commandService);
    }

    [Theory, AutoData]
    public async Task HandleTextMessage_Always_CallsCommandServiceHandleUserReplyByText(BotEntity bot)
    {
        // Arrange
        var param = new CommandParam { MessengerBotId = 1, ChatId = 2, Message = "text" };
        botService.GetAsync(param.MessengerBotId).Returns(Task.FromResult(bot));

        // Act
        await commandController.HandleTextMessage(param);

        // Assert
        await commandService.Received().HandleUserReplyByText(bot, param.ChatId, param.Message);
    }

    [Theory, AutoData]
    public async Task HandleCallbackQuery_Always_CallsCommandServiceHandleUserReplyByButton(BotEntity bot)
    {
        // Arrange
        var param = new CallbackQueryParam { MessengerBotId = 1, ChatId = 2, Message = "{\"id\":5}", MessageId = 3, CallbackId = "4" };
        botService.GetAsync(param.MessengerBotId).Returns(Task.FromResult(bot));

        // Act
        await commandController.HandleCallbackQuery(param);

        // Assert
        await commandService.Received().HandleUserReplyByButton(bot, param.ChatId, param.MessageId, param.CallbackId, Arg.Any<CallbackData>());
    }
}