using Fanex.Data;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace TelegramBot.WebApi.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFanexDataWithSqlServer(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
        {
            var connectionStrings = configuration
                .GetSection("ConnectionStrings")
                .GetChildren()
                .ToDictionary(connection => connection.Key,
                            connection => new ConnectionConfiguration(connection.Key, connection.Value));
            var resourcePath = Path.Combine(env.ContentRootPath, "App_Data", "SPNames");
            DbSettingProviderManager
                .StartNewSession()
                .WithSqlServer(resourcePath)
                .Use(connectionStrings)
                .Run();
            return app;
        }
    }
}
