using Fanex.Logging;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class LoggerServiceTests
{
    private readonly ILogger logger;
    private readonly LoggerService loggerService;

    public LoggerServiceTests()
    {
        logger = Substitute.For<ILogger>();
        loggerService = new LoggerService(logger);
    }

    [Theory, AutoData]
    public async Task Error_Always_CallsErrorAsync(ScheduleJob job, string errorMessage)
    {
        // Act
        await loggerService.Error(job, errorMessage);

        // Assert
        await logger.Received().ErrorAsync(Arg.Any<string>());
    }

    [Theory, AutoData]
    public async Task ErrorWithException_Always_CallsErrorAsync(Exception exception, ScheduleJob job, string errorMessage)
    {
        // Act
        await loggerService.Error(exception, job, errorMessage);

        // Assert
        await logger.Received().ErrorAsync(Arg.Any<string>(), exception);
    }

    [Theory, AutoData]
    public async Task Debug_Always_CallsDebugAsync(ScheduleJob job, string message)
    {
        // Act
        await loggerService.Debug(job, message);

        // Assert
        await logger.Received().DebugAsync(Arg.Any<string>());
    }
}