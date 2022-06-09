using System.Diagnostics.CodeAnalysis;
using WLMonitored.RiskControl.Importer.Services;
using WLMonitored.Shared.Configuration;

namespace WLMonitored.RiskControl.Importer.Extensions;

[ExcludeFromCodeCoverage]
public static class DIExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var scheduleJobs = configuration.GetSection("ScheduleJobs")
            .Get<ScheduleJob[]>()
            .Where(job => job.IsActive)
            .ToDictionary(kvp => kvp.Name, kvp => kvp);

        return services
            .AddSingleton(scheduleJobs)
            .AddSingleton<IWinLossService, WinLossService>()
            .AddSingleton<IPlayerHistoryService, PlayerHistoryService>();
    }
}