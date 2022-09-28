using System.Diagnostics.CodeAnalysis;
using WLMonitored.AccumWLService.Services;
using WLMonitored.Shared.Configuration;

namespace WLMonitored.AccumWLService.Extensions;

[ExcludeFromCodeCoverage]
public static class DIExtensions
{
    public static IServiceCollection AddFanexCachingMemory(this IServiceCollection services)
        => services.AddSingleton<Fanex.Caching.ICacheService, Fanex.Caching.CacheService>();

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<ScheduleJob>(configuration.GetSection(nameof(ScheduleJob)))
            .AddSingleton<ITicketDetailService, TicketDetailService>()
            .AddSingleton<IWinLossService, WinLossService>()
            .AddSingleton<IBetTypeService, BetTypeService>();
}