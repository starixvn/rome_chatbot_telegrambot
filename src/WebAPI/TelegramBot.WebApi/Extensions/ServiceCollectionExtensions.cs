using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using Sentry;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Command;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Notification;
using TelegramBot.Infrastructure.Setting;

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
            services.AddSingleton<IBotClientFactory, BotClientFactory>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IActionService, ActionService>();

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
    }
}