using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using Sentry;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Command;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Notification;
using TelegramBot.Infrastructure.Setting;
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
            services.AddSingleton<IBuilderService, BuilderService>();
            services.AddSingleton<ISenderService, SenderService>();
            services.AddSingleton<ILoggerService, LoggerService>();
            services.AddSingleton<IBotClientFactory, BotClientFactory>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IActionService, ActionService>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();

            return services;
        }

        public static IServiceCollection AddFanexCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRedisCache(configuration.GetSection("Fanex.Caching.Redis"));
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
    }
}