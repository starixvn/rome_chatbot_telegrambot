using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;

namespace WLMonitored.Shared.Jobs;

public abstract class IntervalBackgroundService : BackgroundService
{
    private const int OneSecond = 1000;
    private const int StartMultiplier = 1;
    private const int TwoMultiplier = 2;
    private const int MaxMultiplier = 10;

    protected readonly ScheduleJob scheduleJob;
    protected readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly int umDelayTimeInSeconds;
    private readonly IGlobalSettingService globalSettingService;

    protected IntervalBackgroundService(
        ScheduleJob scheduleJob,
        ILoggerService loggerService,
        IMaintenanceService maintenanceService,
        UMClientConfiguration umClientConfiguration,
        IGlobalSettingService globalSettingService)
    {
        this.scheduleJob = scheduleJob;
        this.loggerService = loggerService;
        this.maintenanceService = maintenanceService;
        umDelayTimeInSeconds = umClientConfiguration.ResultCacheDuration;
        this.globalSettingService = globalSettingService;
    }

    public abstract Task<bool> ExecuteJob(GlobalSetting globalSetting);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!scheduleJob.IsActive)
        {
            return;
        }

        await loggerService.Info($"{scheduleJob.JobInfo} is running.");
        var multiplier = StartMultiplier;

        while (!stoppingToken.IsCancellationRequested)
        {
            var umInfo = await GetUnderMaintenanceTime();

            if (umInfo.IsUnderMaintenanceTime)
            {
                loggerService.InfoConsole("{JobInfo} is Under Maintenance from {From} to {To}.", scheduleJob.JobInfo, umInfo.From, umInfo.To);

                multiplier = umDelayTimeInSeconds;
            }
            else
            {
                try
                {
                    multiplier = await ExecuteAndCalculateMultiplier(multiplier);
                }
                catch (Exception ex)
                {
                    multiplier = StartMultiplier;
                    await loggerService.Error(ex, scheduleJob, nameof(ExecuteAsync));
                }
            }

            await Task.Delay(OneSecond * multiplier, stoppingToken);
        }
    }

    private async Task<int> ExecuteAndCalculateMultiplier(int multiplier)
    {
        var sw = Stopwatch.StartNew();
        var globalSetting = await globalSettingService.GetGlobalSetting(scheduleJob.Environment);

        if (globalSetting.IsStopJob(scheduleJob.Name))
        {
            loggerService.InfoConsole("{JobInfo} is set stopped from DB.", scheduleJob.JobInfo);

            return CalculateMultiplier(false, multiplier);
        }

        bool hasData = await ExecuteJob(globalSetting);
        multiplier = CalculateMultiplier(hasData, multiplier);
        sw.Stop();

        if (hasData)
        {
            loggerService.InfoConsole("{JobInfo} Total execution time {ElapsedMilliseconds} milliseconds", scheduleJob.JobInfo, sw.ElapsedMilliseconds);
        }

        return multiplier;
    }

    private async Task<UnderMaintenanceTime> GetUnderMaintenanceTime()
    {
        try
        {
            return await maintenanceService.GetUnderMaintenanceTimeAsync();
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, scheduleJob, nameof(GetUnderMaintenanceTime), "Could not get UM information.");
        }

        return new UnderMaintenanceTime();
    }

    public virtual int CalculateMultiplier(bool hasData, int multiplier)
    {
        if (scheduleJob.DelayExecutionTimeInSeconds > 0)
        {
            return scheduleJob.DelayExecutionTimeInSeconds;
        }

        if (hasData)
        {
            return StartMultiplier;
        }

        multiplier *= TwoMultiplier;

        return multiplier > MaxMultiplier
            ? MaxMultiplier
            : multiplier;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await loggerService.Info($"{scheduleJob.JobInfo} is stopping.");

        await base.StopAsync(cancellationToken);
    }
}
