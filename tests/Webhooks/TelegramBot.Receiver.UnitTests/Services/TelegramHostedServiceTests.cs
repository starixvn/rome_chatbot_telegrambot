using Fanex.Logging;
using TelegramBot.Client.Models;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.UnitTests.Services;

public class TelegramHostedServiceTests
{
    private readonly ILogger _logger;
    private readonly IHandleUpdateService _handleUpdateService;
    private readonly ReceiverConfiguration _receiverConfiguration;
    private readonly IBotService _botService;
    private readonly TelegramHostedService telegramHostedService;

    public TelegramHostedServiceTests()
    {
        _logger = Substitute.For<ILogger>();
        _handleUpdateService = Substitute.For<IHandleUpdateService>();
        _receiverConfiguration = Substitute.For<ReceiverConfiguration>();
        _botService = Substitute.For<IBotService>();
        telegramHostedService = new TelegramHostedService(_logger, _handleUpdateService, _receiverConfiguration, _botService);
    }

    [Theory, AutoData]
    public async Task StartAsync_Always_CallsGetAllBots(IEnumerable<BotEntity> botEntities)
    {
        // Arrange
        _botService.GetAllAsync().Returns(Task.FromResult(botEntities));

        // Act
        await telegramHostedService.StartAsync(default);

        // Assert

        await _botService.Received().GetAllAsync();
    }

    [Fact]
    public async Task StartAsync_HasException_CallsGetAllBots()
    {
        // Arrange
        var ex = new Exception("ErrorMessage");
        _botService.GetAllAsync().Returns(Task.FromException<IEnumerable<BotEntity>>(ex));

        // Act
        await telegramHostedService.StartAsync(default);

        // Assert

        await _logger.Received().ErrorAsync(ex.Message, ex);
    }

    [Fact]
    public async Task StopAsync_Always_CallsLogInfo()
    {
        // Act
        await telegramHostedService.StopAsync(default);

        // Assert

        await _logger.Received().InfoAsync(Arg.Any<string>());
    }
}