using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Receiver.Controllers;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.UnitTests.Controllers;

public class WebhookControllerTests
{
    private readonly IHandleUpdateService _handleUpdateService;
    private readonly WebhookController webhookController;

    public WebhookControllerTests()
    {
        _handleUpdateService = Substitute.For<IHandleUpdateService>();
        webhookController = new WebhookController(_handleUpdateService);
    }

    [Theory, AutoData]
    public async Task Post_AccessTokenNotExist_ReturnsBadRequest(string accessToken)
    {
        // Act
        var response = await webhookController.Post(accessToken, null);

        // Assert
        Assert.IsType<BadRequestResult>(response);
    }

    [Theory, AutoData]
    public async Task Post_ExistBotClient_ReturnsOK(string accessToken)
    {
        // Act
        var botClient = Substitute.For<ITelegramBotClient>();
        _handleUpdateService.BotClients = new Dictionary<string, ITelegramBotClient> { [accessToken] = botClient };
        Update update = new() { Message = new Message() };
        var response = await webhookController.Post(accessToken, update);

        // Assert
        Assert.IsType<OkResult>(response);
    }
}