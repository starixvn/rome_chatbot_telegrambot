using Microsoft.Extensions.Options;
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Notifications.Shared.Configuration;
using TelegramBot.Notifications.Shared.Constants;
using TelegramBot.Notifications.Shared.Models;
using TelegramBot.Notifications.Shared.Services;

namespace TelegramBot.Notifications.Jobs;

public abstract class NotificationBaseJob : IJob
{
    private readonly IMaintenanceService maintenanceService;
    protected readonly ILoggerService loggerService;
    protected readonly ISystemSettingService systemSettingService;
    protected readonly AppSettings appSettings;

    protected NotificationBaseJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IOptions<AppSettings> options)
    {
        this.maintenanceService = maintenanceService;
        this.loggerService = loggerService;
        this.systemSettingService = systemSettingService;
        appSettings = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var job = GetJobDetails(context);

        if (await IsUM(job))
        {
            return;
        }

        await loggerService.Debug(job, "Start run");

        try
        {
            var systemSetting = await systemSettingService.GetSystemSetting(job.Name, job.Environment);

            if (IsStopJob(systemSetting))
            {
                return;
            }

            await UpdateJobStatusToRunningdAndLastRunTime(job);

            await Execute(job, systemSetting);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job);
        }

        await loggerService.Debug(job, "End run");
    }

    private static ScheduleJob GetJobDetails(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;

        return new ScheduleJob
        {
            Name = dataMap.GetString(JobDataKey.JobName),
            SubAccPermission = dataMap.GetString(JobDataKey.SubAccPermission),
            Environment = dataMap.GetString(JobDataKey.Environment),
            IsGetTestData = dataMap.GetBooleanValue(JobDataKey.IsGetTestData),
            IsActive = true
        };
    }

    private async Task<bool> IsUM(ScheduleJob job)
    {
        try
        {
            var umInfo = await maintenanceService.GetUnderMaintenanceTimeAsync();

            return umInfo.IsUnderMaintenanceTime;
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job, "Could not get UM information.");
        }

        return false;
    }

    private static bool IsStopJob(SystemSetting systemSetting)
        => systemSetting == null || !systemSetting.ServiceFlag;

    private async Task UpdateJobStatusToRunningdAndLastRunTime(ScheduleJob job)
    {
        try
        {
            await systemSettingService.UpdateJobStatusToRunningdAndLastRunTime(job.Name, job.Environment);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job, "Could not update job status to Running and LastRunTime.");
        }
    }

    protected abstract Task Execute(ScheduleJob job, SystemSetting systemSetting);
}