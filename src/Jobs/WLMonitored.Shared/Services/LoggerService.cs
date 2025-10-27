using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;
using Fanex.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using WLMonitored.Shared.Configuration;

namespace WLMonitored.Shared.Services;

public interface ILoggerService
{
    Task Info(string message);

    Task Error(Exception exception, ScheduleJob scheduleJob, string method, string message = "");

    Task Debug(string message);

    void InfoConsole(string message, params object[] args);
}

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public sealed class LoggerService : ILoggerService
{
    private readonly Fanex.Logging.ILogger logger;
    private readonly ILogger<LoggerService> loggerConsole;

    public LoggerService(Fanex.Logging.ILogger logger, ILogger<LoggerService> loggerConsole)
        => (this.logger, this.loggerConsole) = (logger, loggerConsole);

    public Task Info(string message) => logger.InfoAsync(message);

    public Task Error(Exception exception, ScheduleJob scheduleJob, string method, string message = "")
        => logger.ErrorAsync(BuildError(exception, scheduleJob, method, message));

    private static string BuildError(Exception exception, ScheduleJob scheduleJob, string method, string message)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder
            .AppendLine(exception.Message)
            .AppendLine();

        BuildError(errorBuilder, scheduleJob, method, message);

        if (exception is ObjectDbException objectDbException
            && exception.InnerException is SqlException sqlException)
        {
            BuildDbException(errorBuilder, objectDbException, sqlException);
        }

        errorBuilder.Append("EXCEPTION: ").Append(exception.ToString());

        return errorBuilder.ToString();
    }

    private static void BuildError(
        StringBuilder errorBuilder,
        ScheduleJob scheduleJob,
        string method,
        string message)
        => errorBuilder
            .Append(scheduleJob.JobInfo).Append(": ").AppendLine(method)
            .Append("Error: ").AppendLine(message)
            .AppendLine();

    private static void BuildDbException(
        StringBuilder errorBuilder,
        ObjectDbException dbException,
        SqlException sqlException)
    {
        errorBuilder
            .AppendLine("DATABASE INFO:")
            .Append("Server: ").AppendLine(sqlException.Server)
            .Append("DbName: ").AppendLine(dbException.DbName)
            .Append("SpName: ").AppendLine(dbException.SpName)
            .AppendLine("Parameters: ");

        foreach (var item in dbException.Parameters)
        {
            errorBuilder
                .Append(item.Key)
                .Append('=')
                .AppendLine(HttpUtility.HtmlEncode(item.Value.ToString()));
        }

        errorBuilder
            .Append("LineNumber: ").AppendLine(sqlException.LineNumber.ToString())
            .AppendLine();
    }

    public Task Debug(string message)
        => logger.DebugAsync(message);

    public void InfoConsole(string message, params object[] args)
        => loggerConsole.LogInformation(message, args);
}