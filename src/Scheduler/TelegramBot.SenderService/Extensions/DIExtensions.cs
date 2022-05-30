using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration(configuration)
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<ISystemSettingService, SystemSettingService>()
            .AddSingleton<ICustomerService, CustomerService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<IBotService, BotService>();
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

}
