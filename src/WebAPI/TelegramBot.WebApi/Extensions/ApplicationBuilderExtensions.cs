using Fanex.Data;
using Fanex.Data.SqlServer;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;

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
