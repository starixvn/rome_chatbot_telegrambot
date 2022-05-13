using System.Data.SqlClient;
using System.Net;
using System.Text;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Diagnostics;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;

namespace TelegramBot.WebApi.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        private const string BreakLine = "\r\n";
        private const string TwoBreakLine = "\r\n\r\n";

        public static IApplicationBuilder UseFanexDataWithSqlServer(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
        {
            var connectionStrings = configuration
                .GetSection("ConnectionStrings")
                .GetChildren()
                .ToDictionary(connection => connection.Key,
                            connection => new ConnectionConfiguration(connection.Key, connection.Value));
            var resourcePath = Path.Combine(env.ContentRootPath, "App_Data", "SPNames.xml");
            DbSettingProviderManager
                .StartNewSession()
                .WithSqlServer(resourcePath)
                .Use(connectionStrings)
                .Run();
            return app;
        }

        public static IApplicationBuilder UseUM(this IApplicationBuilder app, IConfiguration configuration)
        {
            var umConfig = new UMClientConfiguration().Initialize(configuration);
            UMClientManager
                .UseConfig(umConfig)
                .UseRestClient(new UMRestClient(umConfig))
                .UseHttpContext(new HttpContextHelper(app.ApplicationServices.GetService<IHttpContextAccessor>()));
            return app;
        }


        public static IApplicationBuilder ConfigureExceptionHandler(this IApplicationBuilder app, bool isDevelopment, Fanex.Logging.ILogger logger)
        => app.UseExceptionHandler(configure => configure.Run(context => LogError(context, isDevelopment, logger)));

        private static async Task LogError(HttpContext context, bool isDevelopment, Fanex.Logging.ILogger logger)
        {
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentType = "application/json";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

            if (contextFeature != null)
            {
                var exception = contextFeature.Error;
                await logger.ErrorAsync(await BuildError(context.Request, exception), exception);
            }
        }

        private static async Task<string> BuildError(HttpRequest request, Exception exception)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder
                .Append(exception.Message).Append(TwoBreakLine)
                .Append("PATH: ")
                .Append($"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}")
                .Append(TwoBreakLine)
                .Append("BODY: ")
                .Append(await GetRawBodyStringAsync(request))
                .Append(TwoBreakLine)
                .Append("HEADERS:").Append(BreakLine)
                .Append(GetHeaderValues(request))
                .Append(TwoBreakLine)
                .Append("COOKIES:").Append(BreakLine)
                .Append(GetCookiesValues(request))
                .Append(TwoBreakLine);

            if (exception is ObjectDbException objectDbException
                && exception.InnerException is SqlException sqlException)
            {
                errorBuilder.Append(BuildDbException(objectDbException, sqlException));
            }

            errorBuilder
                .Append("EXCEPTION: ")
                .Append(exception.ToString());

            return errorBuilder.ToString();
        }

        private static async Task<string> GetRawBodyStringAsync(HttpRequest request)
        {
            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static string GetHeaderValues(HttpRequest request)
            => string.Join(BreakLine, request.Headers.Select(header => $"{header.Key}:{header.Value}"));

        private static string GetCookiesValues(HttpRequest request)
            => string.Join(BreakLine, request.Cookies.Select(header => $"{header.Key}:{header.Value}"));

        private static string BuildDbException(ObjectDbException dbException, SqlException sqlException)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder
                .Append("DATABASE INFO:").Append(BreakLine)
                .Append("Server: ").Append(sqlException.Server).Append(BreakLine)
                .Append("DbName: ").Append(dbException.DbName).Append(BreakLine)
                .Append("SpName: ").Append(dbException.SpName).Append(BreakLine)
                .Append("Parameters: ").Append(BreakLine).Append(GetSpParameters(dbException.Parameters)).Append(BreakLine)
                .Append("Line: ").Append(sqlException.LineNumber).Append(TwoBreakLine);

            return errorBuilder.ToString();
        }

        private static string GetSpParameters(IDictionary<string, object> parameters)
            => string.Join(BreakLine, parameters.Select(p => $"{p.Key}={p.Value}"));
    }
}
