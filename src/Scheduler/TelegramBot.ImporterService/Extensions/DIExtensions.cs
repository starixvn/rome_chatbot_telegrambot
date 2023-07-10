using System.Diagnostics.CodeAnalysis;
using System.Net;
using TelegramBot.Importer.Repositories;
using TelegramBot.Importer.Services;
using ClientServices = Alpha.API.Client.Services;

namespace TelegramBot.ImporterService.Extensions;

[ExcludeFromCodeCoverage]
public static class DIExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
        => services.AddSingleton<IAnnouncementRepository, AnnouncementRepository>();

    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<ICacheRefService, CacheRefService>()
            .AddScoped<ISystemSettingService, SystemSettingService>()
            .AddScoped<ICustomerService, CustomerService>()
            .AddSingleton<IProductService, ProductService>()
            .AddSingleton<IPersonalMessageService, PersonalMessageService>()
            .AddSingleton<IYesterdayWinLossService, YesterdayWinLossService>()
            .AddSingleton<IProblemAccountService, ProblemAccountService>()
            .AddSingleton<IMemberWinLossLimitService, MemberWinLossLimitService>()
            .AddSingleton<IAnnouncementService, AnnouncementService>()
            .AddSingleton<IMyWatchlistService, MyWatchlistService>()
            .AddSingleton<IAbnormalLoginService, AbnormalLoginService>();

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
