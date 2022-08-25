using Fanex.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.UnitTests.Services;

public class HandleUpdateServiceTests
{
    private readonly ILogger _logger;
    private readonly ICommandService _commandService;
    private readonly ISettingService _settingService;
    private readonly HandleUpdateService handleUpdateService;
    private readonly ITelegramBotClient botClient;

    public HandleUpdateServiceTests()
    {
        _logger = Substitute.For<ILogger>();
        _commandService = Substitute.For<ICommandService>();
        _settingService = Substitute.For<ISettingService>();
        handleUpdateService = new HandleUpdateService(_logger, _commandService, _settingService);
        botClient = Substitute.For<ITelegramBotClient>();
        botClient.BotId.Returns(1);
    }

    [Fact]
    public async Task HandleUpdateAsync_MessageIsStartCommand_CallsSettingServiceStartAsync()
    {
        // Arrange
        Update update = new() { Message = new Message() { Text = "/start", Chat = new Chat() { Id = 2 } } };

        // Act
        await handleUpdateService.HandleUpdateAsync(botClient, update, default);

        // Assert
        await _settingService.Received().StartAsync(1, 2, "/start");
    }

    [Theory, AutoData]
    public async Task HandleUpdateAsync_OtherTextMessage_CallsHandleTextMessage(string message)
    {
        // Arrange
        Update update = new() { Message = new Message() { Text = message, Chat = new Chat() { Id = 2 } } };

        // Act
        await handleUpdateService.HandleUpdateAsync(botClient, update, default);

        // Assert
        await _commandService.Received().HandleTextMessage(1, 2, message);
    }
}