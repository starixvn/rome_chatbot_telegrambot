using System.Diagnostics.CodeAnalysis;
using Fanex.Caching;
using Fanex.Caching.Memory;
using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using MemberAPIs.Services;
using Sentry;
using StackExchange.Redis;

namespace MemberAPIs.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            _ = services.AddSingleton<ILoginFailedNotifierService, LoginFailedNotifierService>();
            return services;
        }

        public static IServiceCollection AddFanexCaching(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddSingleton<ICacheServiceFactory, CacheServiceFactory>()
                .AddSingleton(new MemoryCacheOptions())
                .AddSingleton<MemoryCache>()
                .AddSingleton<ICache, MemoryCache>(s => s.GetService<MemoryCache>());

            RedisCacheOptions options = new RedisCacheOptions();
            IConfigurationSection redisConfiguration = configuration.GetSection("Fanex.Caching.Redis");
            redisConfiguration.Bind(options);
            ReadEndPoints(redisConfiguration, options);
            _ = services.AddSingleton(options)
                .AddSingleton<RedisCache>()
                .AddSingleton<ICache, RedisCache>(s => s.GetService<RedisCache>());

            return services;
        }

        public static IServiceCollection AddFanexLogging(this IServiceCollection services, IConfiguration configuration)
        {
            List<ILogEngine> logEngines = new List<ILogEngine>
            {
                new RabbitMQLogging(configuration["Fanex.Logging:RabbitMQConnectionStrings"])
            };

            string sentryUrl = configuration["Fanex.Logging:SentryUrl"];
            if (!string.IsNullOrWhiteSpace(sentryUrl))
            {
                logEngines.Add(new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(sentryUrl)
                }));
            }

            _ = LogManager
                .SetDefaultLogCategory(configuration["Fanex.Logging:DefaultCategory"])
                .Use(logEngines);

            _ = services.AddSingleton(Logger.Log);

            _ = ExceptionHandler
                .UseConfig(new LoggingConfiguration()
                .GetHttpHeaderInfo()
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo));

            return services;
        }

        public static IServiceCollection AddOneBookUm(this IServiceCollection services, IConfiguration configuration)
        {
            UMClientConfiguration umConfig = configuration.GetSection("ONEbook.UM").Get<UMClientConfiguration>();
            _ = services.AddSingleton(umConfig)
                .AddSingleton<ILoggerService, LoggerService>()
                .AddSingleton<IPHelper, IPHelperImpl>()
                .AddSingleton<IMaintenanceService, MaintenanceService>()
                .AddSingleton<IUMCacheService, UMCacheService>()
                .AddSingleton<IUMRestClient, UMRestClient>();

            return services;
        }

        private static void ReadEndPoints(IConfigurationSection configuration, RedisCacheOptions options)
        {
            string[] endPoints = configuration.GetSection("ConnectionOptions:EndPoints").Get<string[]>();

            if (endPoints == null || endPoints.Length <= 0)
            {
                return;
            }

            options.ConnectionOptions ??= new ConfigurationOptions();

            foreach (string endPoint in endPoints)
            {
                options.ConnectionOptions.EndPoints.Add(endPoint);
            }
        }

        public static IServiceCollection AddFilters(this IServiceCollection services) => services;
    }
}