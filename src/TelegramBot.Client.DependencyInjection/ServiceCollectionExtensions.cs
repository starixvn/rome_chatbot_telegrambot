using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            services.AddHttpClient<BotService>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(clientConfiguration.Uri);
                httpClient.Timeout = TimeSpan.FromMinutes(5);
            });
            return services;
        }
    }
}
