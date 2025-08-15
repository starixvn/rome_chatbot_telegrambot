using System.Diagnostics.CodeAnalysis;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Builder;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using Sentry;

namespace TelegramBot.SenderService.Extensions;

// Justification: This class contains only extension methods and is excluded from code coverage for testability reasons.
[ExcludeFromCodeCoverage(Justification = "Contains only extension methods; excluded from code coverage for testability reasons.")]
public static class FanexLibsExtensions
{
    public static IServiceCollection AddFanexDataWithSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFanexSql(configuration);
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        return services;
    }

    public static IServiceCollection AddFanexCachingMemory(this IServiceCollection services)
        => services.AddSingleton<Fanex.Caching.ICacheService, Fanex.Caching.CacheService>();

    public static IServiceCollection AddFanexLogging(this IServiceCollection services, IConfiguration configuration)
    {
        LogManager
            .SetDefaultLogCategory(configuration["Fanex.Logging:DefaultCategory"])
            .Use(GetLogEngines(configuration));

        ExceptionHandler
            // To use custom config
            .UseConfig(new LoggingConfiguration()
            // To ignore exceptions
            // .IgnoreException<DivideByZeroException>()                
            );

        return services.AddSingleton(Logger.Log);
    }

    private static IEnumerable<ILogEngine> GetLogEngines(IConfiguration configuration)
    {
        yield return new RabbitMQLogging(configuration["Fanex.Logging:RabbitMQConnectionStrings"]);

        var sentryUrl = configuration["Fanex.Logging:SentryDsn"];
        if (!string.IsNullOrWhiteSpace(sentryUrl))
        {
            yield return new SentryLogging(new SentryEngineOptions
            {
                Dsn = new Dsn(sentryUrl)
            });
        }
    }

    public static IServiceCollection AddONEBookUM(this IServiceCollection services, IConfiguration configuration)
    {
        var umConfig = configuration.GetSection("ONEbook.UM").Get<UMClientConfiguration>();

        return services
            .AddSingleton(umConfig)
            .AddSingleton<IPHelper, IPHelperImpl>()
            .AddSingleton<IMaintenanceService, MaintenanceService>()
            .AddSingleton<IUMCacheService, UMCacheService>()
            .AddSingleton<IUMRestClient, UMRestClient>();
    }

    public static IApplicationBuilder UseONEBookUM(this IApplicationBuilder app, IConfiguration configuration)
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);
        UMClientManager
            .UseConfig(umConfig)
            .UseRestClient(new UMRestClient(umConfig));

        return app;
    }
}