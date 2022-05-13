using System.Data.SqlClient;
using System.Text;
using Fanex.Data;

namespace TelegramBot.SenderService.Services;

public interface ILoggerService
{
    Task Error(string jobName, string environment, string errorMessage);

    Task Error(Exception exception, string jobName, string environment, string errorMessage = "");

    Task Debug(string message);
}

public class LoggerService : ILoggerService
{
    private const string BreakLine = "\r\n";
    private const string TwoBreakLine = "\r\n\r\n";

    private readonly Fanex.Logging.ILogger logger;

    public LoggerService(Fanex.Logging.ILogger logger)
        => this.logger = logger;

    public Task Error(string jobName, string environment, string errorMessage)
    {
        var errorBuilder = new StringBuilder();
        BuildJobNameError(errorBuilder, jobName, environment, errorMessage);

        return logger.ErrorAsync(errorBuilder.ToString());
    }

    public Task Error(Exception exception, string jobName, string environment, string errorMessage = "")
        => logger.ErrorAsync(BuildError(exception, jobName, environment, errorMessage), exception);

    public static string BuildError(Exception exception, string jobName, string environment, string errorMessage)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder.Append(exception.Message).Append(TwoBreakLine);

        BuildJobNameError(errorBuilder, jobName, environment, errorMessage);

        if (exception is ObjectDbException objectDbException
            && exception.InnerException is SqlException sqlException)
        {
            BuildDbException(errorBuilder, objectDbException, sqlException);
        }

        errorBuilder.Append("EXCEPTION: ").Append(exception.ToString());

        return errorBuilder.ToString();
    }

    private static void BuildJobNameError(
        StringBuilder errorBuilder,
        string jobName,
        string environment,
        string errorMessage)
    {
        errorBuilder.Append("Error Message: ").Append(errorMessage).Append(BreakLine);
        errorBuilder.Append("Job Name: ").Append(jobName).Append(BreakLine);
        errorBuilder.Append("Environment: ").Append(environment).Append(TwoBreakLine);
    }

    private static void BuildDbException(
        StringBuilder errorBuilder,
        ObjectDbException dbException,
        SqlException sqlException)
    {
        errorBuilder
            .Append("DATABASE INFO:").Append(BreakLine)
            .Append("Server: ").Append(sqlException.Server).Append(BreakLine)
            .Append("DbName: ").Append(dbException.DbName).Append(BreakLine)
            .Append("SpName: ").Append(dbException.SpName).Append(BreakLine)
            .Append("Parameters: ").Append(BreakLine).Append(GetSpParameters(dbException.Parameters)).Append(BreakLine)
            .Append("Line: ").Append(sqlException.LineNumber).Append(TwoBreakLine);
    }

    private static string GetSpParameters(IDictionary<string, object> parameters)
        => string.Join(BreakLine, parameters.Select(p => $"{p.Key}={p.Value}"));

    public Task Debug(string message) => logger.DebugAsync(message);
}