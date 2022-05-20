using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Services;

namespace TelegramBot.ImporterService.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<AppSettings>(configuration)
            .AddSingleton<ILoggerService, LoggerService>()
            .AddScoped<ISystemSettingService, SystemSettingService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddSingleton<IPersonalMessageService, PersonalMessageService>();
}
