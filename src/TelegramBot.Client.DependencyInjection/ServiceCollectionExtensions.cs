using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using TelegramBot.Client.Services;

namespace TelegramBot.Client.DependencyInjection
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
            return services;
        }

        private static void ConfigureHttpClient(HttpClient httpClient, ClientConfiguration clientConfiguration)
        {
            httpClient.BaseAddress = new Uri(clientConfiguration.Uri);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        }
    }
}
