using System.Threading.Tasks;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Jobs;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;

namespace WLMonitored.Shared.UnitTests.Jobs;

public class NoExceptionBackgroundService : IntervalBackgroundService
{
    public NoExceptionBackgroundService(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IGlobalSettingService globalSettingService)
        : base(scheduleJob, loggerService, maintenanceService, umClientConfiguration, globalSettingService)
    {
    }

    public override Task<bool> ExecuteJob(GlobalSetting globalSetting)
        => Task.FromResult(true);
}