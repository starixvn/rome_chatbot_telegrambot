using System.Diagnostics.CodeAnalysis;
using Quartz;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Jobs;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.Extensions;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration(configuration)
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<ISystemSettingService, SystemSettingService>()
            .AddSingleton<ICustomerService, CustomerService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<IBotService, BotService>()
            .AddTransient<IJobListener, RetryJobListener>()
            .AddSingleton<IMessageSender, MessageSender>();
    private static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings
        {
            IsTest = configuration.GetSection("IsTest").Get<bool>(),
            ProxyAddress = configuration.GetSection("ProxyAddress").Get<string>(),
            FunctionPermissions = configuration.GetSection("FunctionPermissions")
                .Get<FunctionPermission[]>()
                .ToDictionary(kvp => kvp.FunctionKey, kvp => kvp.SubAccPermission)
        };

        return services.AddSingleton(appSettings);
    }
}
