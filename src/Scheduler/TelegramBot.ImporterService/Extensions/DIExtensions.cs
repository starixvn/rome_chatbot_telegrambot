using System.Net;
using TelegramBot.Importer.Services;
using ClientServices = Alpha.API.Client.Services;

namespace TelegramBot.ImporterService.Extensions;

public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddSingleton<ILoggerService, LoggerService>()
            .AddScoped<ISystemSettingService, SystemSettingService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddSingleton<IPersonalMessageService, PersonalMessageService>();
    
    public static IServiceCollection AddAlphaServicesClient(this IServiceCollection services, IConfiguration configuration)
    {        
        services
            .AddHttpClient<ClientServices.IWinLossService, ClientServices.WinLossService>(httpClient
                => httpClient.BaseAddress = new Uri(configuration.GetValue<string>("AlphaAPIBaseUri")))
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

        return services;
    }
}
