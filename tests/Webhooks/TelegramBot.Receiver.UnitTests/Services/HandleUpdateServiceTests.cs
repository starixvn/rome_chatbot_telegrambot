using Fanex.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.UnitTests.Services;

public class HandleUpdateServiceTests
{
    private readonly ILogger _logger;
    private readonly ReceiverConfiguration _botConfiguration;
    private readonly ICommandService _commandService;
    private readonly ISettingService _settingService;
    private readonly HandleUpdateService handleUpdateService;
    private readonly ITelegramBotClient botClient;

    public HandleUpdateServiceTests()
    {
        _logger = Substitute.For<ILogger>();
        _botConfiguration = Substitute.For<ReceiverConfiguration>();
        _commandService = Substitute.For<ICommandService>();
        _settingService = Substitute.For<ISettingService>();
        handleUpdateService = new HandleUpdateService(_logger, _botConfiguration, _commandService, _settingService);
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

    [Fact]
    public async Task HandleUpdateAsync_CallbackQuery_CallsHandleCallbackMessage()
    {
        // Arrange
        var callbackQuery = new CallbackQuery() { Id = "CallbackId", Data = "CallbackData", Message = new Message() { MessageId = 3, Chat = new Chat() { Id = 2 } } };
        Update update = new() { CallbackQuery = callbackQuery };

        // Act
        await handleUpdateService.HandleUpdateAsync(botClient, update, default);

        // Assert
        await _commandService.Received().HandleCallbackMessage(1,
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            callbackQuery.Id,
            callbackQuery.Data);
    }

    [Fact]
    public async Task HandleUpdateAsync_HasException_CallsLogError()
    {
        // Arrange
        Update update = new() { Message = new Message() { Text = "/start", Chat = new Chat() { Id = 2 } } };
        var ex = new Exception("ErrorMessage");
        _settingService.StartAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<Result>(ex));

        // Act
        await handleUpdateService.HandleUpdateAsync(botClient, update, default);

        // Assert
        await _logger.Received().ErrorAsync(Arg.Any<string>());
    }
}