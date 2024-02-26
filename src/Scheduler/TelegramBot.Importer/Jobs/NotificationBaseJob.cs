using System.Diagnostics;
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

[DisallowConcurrentExecution]
public abstract class NotificationBaseJob : IJob
{
    private readonly IMaintenanceService maintenanceService;
    protected readonly ILoggerService loggerService;
    protected readonly ISystemSettingService systemSettingService;
    protected readonly ICustomerService customerService;

    protected NotificationBaseJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService)
    {
        this.maintenanceService = maintenanceService;
        this.loggerService = loggerService;
        this.systemSettingService = systemSettingService;
        this.customerService = customerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        //var sw = Stopwatch.StartNew();
        var job = GetJobDetails(context);

        if (await IsUM(job))
        {
            loggerService.InfoConsole(job, "is Under Maintenance.");
            return;
        }

        try
        {
            var systemSetting = await systemSettingService.GetSystemSetting(job.Name, job.Environment);

            if (systemSetting == null || !systemSetting.ServiceFlag)
            {
                loggerService.InfoConsole(job, "is set stopped from DB.");
                return;
            }

            await UpdateJobStatusToRunning(job);

            await Execute(job, systemSetting);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            await loggerService.Error(ex, job);
        }

        //sw.Stop();
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
            IsActive = true,
            SiteDBs = dataMap.GetString(JobDataKey.SiteDBs),
            MonitorEnvironment = dataMap.GetString(JobDataKey.MonitorEnvironment),
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

    protected Task<IEnumerable<LinkedCustomer>> GetLinkedCustomers(string custIds, ScheduleJob job, string sites = null)
        => customerService.GetLinkedCustomers(job.Name, custIds, job.Environment, sites);

    protected Task<IEnumerable<CustomerStatus>> GetCustomerStatus(IEnumerable<LinkedCustomer> linkedCustomers)
    {
        var customerStatusParams = linkedCustomers.Select(c => new CustomerStatusParam(c.CustId, c.SubAccId));

        return customerService.GetCustomerStatus(customerStatusParams);
    }

    public abstract Task Execute(ScheduleJob job, SystemSetting systemSetting);

    public Task UpdateJobStatusToCompleted(ScheduleJob job, long maxLogId)
    {
        var jobStatus = JobStatusConstants.Completed;
        var lastRunTime = DateTime.Now;

        try
        {
            return systemSettingService.UpdateJobStatus(job.Name, job.Environment, jobStatus, lastRunTime, maxLogId);
        }
        catch (Exception ex)
        {
            return loggerService.Error(ex, job, $"Could not update job status to {jobStatus} and LastRunTime = {lastRunTime}.");
        }
    }
}