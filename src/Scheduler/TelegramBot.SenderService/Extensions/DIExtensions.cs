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
            .AddScoped<INotificationService, NotificationService>();

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
