using System.Data.SqlClient;
using System.Runtime.Serialization;
using Fanex.Data;
using Microsoft.Extensions.Logging;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class LoggerServiceTests
{
    private readonly Fanex.Logging.ILogger logger;
    private readonly ILogger<LoggerService> loggerConsole;
    private readonly LoggerService loggerService;

    public LoggerServiceTests()
    {
        logger = Substitute.For<Fanex.Logging.ILogger>();
        loggerConsole = Substitute.For<ILogger<LoggerService>>();
        loggerService = new LoggerService(logger, loggerConsole);
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
    public async Task ErrorWithException_Always_CallsErrorAsync(
        ScheduleJob job,
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
        await loggerService.Error(exception, job, errorMessage);

        // Assert
        await logger.Received().ErrorAsync(Arg.Any<string>(), exception);
    }
}