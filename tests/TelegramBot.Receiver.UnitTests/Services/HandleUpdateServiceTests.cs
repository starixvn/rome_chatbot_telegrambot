using Fanex.Logging;
using Telegram.Bot;
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
    }
}