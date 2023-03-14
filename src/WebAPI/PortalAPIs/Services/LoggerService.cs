using System.Data.SqlClient;
using System.Text;
using System.Web;
using Fanex.Data;

namespace PortalAPIs.Services;

public interface ILoggerService
{
    Task Error(Exception exception);
    Task Info(string message);
}

public sealed class LoggerService : ILoggerService
{
    private readonly Fanex.Logging.ILogger logger;

    public LoggerService(Fanex.Logging.ILogger logger)
        => this.logger = logger;


    public Task Error(Exception exception)
        => logger.ErrorAsync(BuildError(exception));

    private static string BuildError(Exception exception)
    {
        var errorBuilder = new StringBuilder();
        errorBuilder
            .AppendLine(exception.Message)
            .AppendLine();

        if (exception is ObjectDbException objectDbException
            && exception.InnerException is SqlException sqlException)
        {
            BuildDbException(errorBuilder, objectDbException, sqlException);
        }

        errorBuilder.Append("EXCEPTION: ").Append(exception.ToString());

        return errorBuilder.ToString();
    }

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

    public async Task Info(string message)
    {
        await logger.InfoAsync(message);
    }
}