using Fanex.Data;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;
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
    }
}
