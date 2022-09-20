using System.Data.SqlClient;
using System.Runtime.Serialization;
using Fanex.Data;
using Fanex.Logging;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Services;

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
    public async Task Error_Always_CallsErrorAsync(string jobName, string environment, string errorMessage)
    {
        // Act
        await loggerService.Error(jobName, environment, errorMessage);

        // Assert
        await logger.Received().ErrorAsync(Arg.Any<string>());
    }

    [Theory, AutoData]
    public async Task ErrorWithException_Always_CallsErrorAsync(
        string jobName,
        string environment,
        string errorMessage,
        string settingKey,
        string spName,
        string dbName,
        string connectionString,
        int commandTimeout,
        IDictionary<string, string> columnMaps)
    {
        // Arrange
        var exception = new ObjectDbException(
            FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException,
            settingKey,
            spName,
            dbName,
            connectionString,
            commandTimeout,
            columnMaps);

        // Act
        await loggerService.Error(exception, jobName, environment, errorMessage);

        // Assert
        await logger.Received().ErrorAsync(Arg.Any<string>(), exception);
    }

    [Theory, AutoData]
    public async Task Debug_Always_CallsDebugAsync(string message)
    {
        // Act
        await loggerService.Debug(message);

        // Assert
        await logger.Received().DebugAsync(message);
    }
}