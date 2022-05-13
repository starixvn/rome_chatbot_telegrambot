using TelegramBot.Notifications.Messages.Services;
using TelegramBot.Notifications.Shared.Configuration;
using TelegramBot.Notifications.Shared.Services;

namespace TelegramBot.ImporterService.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<AppSettings>(configuration)
            .AddSingleton<ILoggerService, LoggerService>()
            .AddScoped<ISystemSettingService, SystemSettingService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddSingleton<IMessageService, MessageService>()
            .AddScoped<INotificationService, NotificationService>();
}
