using System.Diagnostics.CodeAnalysis;
using WLMonitored.RiskControl.Importer.Services;
using WLMonitored.Shared.Configuration;

namespace WLMonitored.RiskControl.Importer.Extensions;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
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