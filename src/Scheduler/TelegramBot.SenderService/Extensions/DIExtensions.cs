using System.Net;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration(configuration)
            .AddSingleton<ILoggerService, LoggerService>()
            .AddScoped<ISystemSettingService, SystemSettingService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddScoped<INotificationService, NotificationService>()
            .AddTelegramClient(configuration);
    private static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings
        {
            FunctionPermissions = configuration.GetSection("FunctionPermissions")
                .Get<FunctionPermission[]>()
                .ToDictionary(kvp => kvp.FunctionKey, kvp => kvp.SubAccPermission)
        };

        return services.AddSingleton(appSettings);
    }

    private static IServiceCollection AddTelegramClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient<TelegramBot.Client.Services.INotificationService, TelegramBot.Client.Services.NotificationService>(
                httpClient => httpClient.BaseAddress = new Uri(configuration.GetValue<string>("TelegramBotClientUri")))
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

        return services;
    }
}
