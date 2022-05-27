using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public abstract class NotificationBaseJob : IJob
{
    private readonly IMaintenanceService maintenanceService;
    protected readonly ILoggerService loggerService;
    protected readonly ISystemSettingService systemSettingService;

    protected NotificationBaseJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService)
    {
        this.maintenanceService = maintenanceService;
        this.loggerService = loggerService;
        this.systemSettingService = systemSettingService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var job = GetJobDetails(context);
        
        await loggerService.Debug(job, "Start run");

        if (await IsUM(job))
        {
            return;
        }

        try
        {
            var systemSetting = await systemSettingService.GetSystemSetting(job.Name, job.Environment);

            if (systemSetting == null || !systemSetting.ServiceFlag)
            {
                return;
            }

            await UpdateJobStatusToRunning(job);

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

    protected Task UpdateJobStatusToRunning(ScheduleJob job)
        => UpdateJobStatusAndLastRunTime(job, "Running", DateTime.Now);

    private Task UpdateJobStatusAndLastRunTime(ScheduleJob job, string jobStatus, DateTime lastRunTime)
    {
        try
        {
            return systemSettingService.UpdateJobStatusAndLastRunTime(job.Name, job.Environment, jobStatus, lastRunTime);
        }
        catch (Exception ex)
        {
            return loggerService.Error(ex, job, $"Could not update job status to {jobStatus} and LastRunTime = {lastRunTime}.");
        }
    }

    protected abstract Task Execute(ScheduleJob job, SystemSetting systemSetting);
}