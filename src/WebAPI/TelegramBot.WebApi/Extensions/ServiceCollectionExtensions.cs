using Fanex.Caching;
using Fanex.Caching.Memory;
using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using Sentry;
using StackExchange.Redis;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Action.Validators;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Command;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Notification;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.WebApi.Filters;

namespace TelegramBot.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<ISettingService, SettingService>();
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IMessageBuilder, MessageBuilder>();
            services.AddSingleton<IMessageSender, MessageSender>();
            services.AddSingleton<IMessageLogger, MessageLogger>();
            services.AddSingleton<IBotClientFactory, BotClientFactory>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IActionService, ActionService>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
            services.AddSingleton<IFunctionService, FunctionService>();
            services.AddSingleton<IMessageFormatter, TelegramFormatter>();
            services.AddSingleton<IResourceService, ResourceService>();
            services.AddSingleton<IActionValidatorFactory, ActionValidatorFactory>();
            services.AddFunctionHandlers();

            return services;
        }

        public static IServiceCollection AddFanexCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICacheServiceFactory, CacheServiceFactory>();

            services.AddSingleton(new MemoryCacheOptions());
            services.AddSingleton<MemoryCache>()
                    .AddSingleton<ICache, MemoryCache>(s => s.GetService<MemoryCache>());

            var options = new RedisCacheOptions();
            var redisConfiguration = configuration.GetSection("Fanex.Caching.Redis");
            redisConfiguration.Bind(options);
            ReadEndPoints(redisConfiguration, options);
            services.AddSingleton(options);
            services.AddSingleton<RedisCache>()
                    .AddSingleton<ICache, RedisCache>(s => s.GetService<RedisCache>());

            return services;
        }

        public static IServiceCollection AddFanexLogging(this IServiceCollection services, IConfiguration configuration)
        {
            var logEngines = new List<ILogEngine>
            {
                new RabbitMQLogging(configuration["Fanex.Logging:RabbitMQConnectionStrings"])
            };

            var sentryUrl = configuration["Fanex.Logging:SentryUrl"];
            if (!string.IsNullOrWhiteSpace(sentryUrl))
            {
                logEngines.Add(new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(sentryUrl)
                }));
            }

            LogManager
                    .SetDefaultLogCategory(configuration["Fanex.Logging:DefaultCategory"])
                    .Use(logEngines);

            services.AddSingleton(Logger.Log);
            return services;
        }

        public static IServiceCollection AddFilters(this IServiceCollection services)
        {
            services.AddScoped<AutoReplyUMFilterAttribute>();
            services.AddScoped<AccountStatusFilterAttribute>();

            return services;
        }

        public static IServiceCollection AddOneBookUm(this IServiceCollection services, IConfiguration configuration)
        {
            var umConfig = configuration.GetSection("ONEbook.UM").Get<UMClientConfiguration>();
            services.AddSingleton(umConfig);
            services.AddSingleton<IPHelper, IPHelperImpl>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
            services.AddSingleton<IUMCacheService, UMCacheService>();
            services.AddSingleton<IUMRestClient, UMRestClient>();

            return services;
        }

        public static IServiceCollection AddFunctionHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IFunctionHandlerFactory, FunctionHandlerFactory>();
            services.AddSingleton<OustandingFunctionHandler>()
                .AddSingleton<IFunctionHandler, OustandingFunctionHandler>(s => s.GetService<OustandingFunctionHandler>());
            return services;
        }

        private static void ReadEndPoints(IConfigurationSection configuration, RedisCacheOptions options)
        {
            var endPoints = configuration.GetSection("ConnectionOptions:EndPoints").Get<string[]>();

            if (endPoints != null && endPoints.Length > 0)
            {
                options.ConnectionOptions = options.ConnectionOptions ?? new ConfigurationOptions();

                foreach (var endPoint in endPoints)
                {
                    options.ConnectionOptions.EndPoints.Add(endPoint);
                }
            }
        }
    }
}