using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry;
using Sentry;
using TelegramBot.Client.Services;
using TelegramBot.Receiver.Configurations;

namespace TelegramBot.Receiver.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClient(this IServiceCollection services, IConfiguration configuration)
        {
            var clientConfiguration = new ClientConfiguration();
            configuration.GetSection(nameof(ClientConfiguration)).Bind(clientConfiguration);
            services.AddSingleton(clientConfiguration);
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<ISettingService, SettingService>();
            services.AddSingleton<ICommandService, CommandService>();
            services.AddHttpClient<IBotService, BotService>(httpClient => ConfigureHttpClient(httpClient, clientConfiguration));
            services.AddHttpClient<ISettingService, SettingService>(httpClient => ConfigureHttpClient(httpClient, clientConfiguration));
            services.AddHttpClient<ICommandService, CommandService>(httpClient => ConfigureHttpClient(httpClient, clientConfiguration));
            services.AddHttpClient<INotificationService, NotificationService>(httpClient => ConfigureHttpClient(httpClient, clientConfiguration));
            return services;
        }

        private static void ConfigureHttpClient(HttpClient httpClient, ClientConfiguration clientConfiguration)
        {
            httpClient.BaseAddress = new Uri(clientConfiguration.WebApiUri);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
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