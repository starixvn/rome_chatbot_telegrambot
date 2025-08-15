using System.Diagnostics.CodeAnalysis;
using Fanex.Logging;
using TelegramBot.Client.Services;

namespace TelegramBot.Receiver.DependencyInjection
{
    [ExcludeFromCodeCoverage(Justification = "This class only contains DI extension methods and does not require code coverage.")]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<ISettingService, SettingService>();
            services.AddSingleton<ICommandService, CommandService>();

            var chatbotAPIUri = configuration.GetSection("ClientConfiguration:WebApiUri").Get<string>();
            services.AddHttpClient<IBotService, BotService>(httpClient => ConfigureHttpClient(httpClient, chatbotAPIUri));
            services.AddHttpClient<ISettingService, SettingService>(httpClient => ConfigureHttpClient(httpClient, chatbotAPIUri));
            services.AddHttpClient<ICommandService, CommandService>(httpClient => ConfigureHttpClient(httpClient, chatbotAPIUri));

            return services;
        }

        private static void ConfigureHttpClient(HttpClient httpClient, string chatbotAPIUri)
        {
            httpClient.BaseAddress = new Uri(chatbotAPIUri);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public static IServiceCollection AddFanexLogging(this IServiceCollection services)
        {
            services.AddSingleton(Logger.Log);
            return services;
        }
    }
}