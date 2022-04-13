using Fanex.Caching.Redis;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Command;
using TelegramBot.Infrastructure.Message;
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
            services.AddSingleton<ISenderService, SenderService>();
            services.AddSingleton<IBotClientFactory, BotClientFactory>();
            return services;
        }

        public static IServiceCollection AddFanexCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRedisCache(configuration.GetSection("Fanex.Caching.Redis"));
            return services;
        }
    }
}
