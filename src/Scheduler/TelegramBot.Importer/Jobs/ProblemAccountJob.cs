
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.ProblemAccounts;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class ProblemAccountJob : NotificationBaseJob
{
    private readonly IProblemAccountService problemAccountService;

    public ProblemAccountJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IProblemAccountService problemAccountService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
        => this.problemAccountService = problemAccountService;

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<ProblemAccountNotification>();

        var (maxLogId, problemAccounts) = await problemAccountService.GetProblemAccounts(systemSetting.MaxLogId, systemSetting.BatchSize);

        if (problemAccounts.Any())
        {
            var groupProblemAccounts = problemAccountService.BuildGroupProblemAccounts(problemAccounts);

            var custIds = groupProblemAccounts.Select(pa => pa.CustId).JoinWithComma();
            var linkedCustomers = await GetLinkedCustomers(custIds, job);

            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);

                notifications = BuildProblemAccountNotifications(
                    job,
                    groupProblemAccounts,
                    linkedCustomers,
                    customerStatus);
            }
        }

        // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
        var result = await problemAccountService.Import(
            notifications,
            job.Environment,
            maxLogId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private IEnumerable<ProblemAccountNotification> BuildProblemAccountNotifications(
        ScheduleJob job,
        IEnumerable<GroupProblemAccount> groupProblemAccounts,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        var identity = 1;

        var notifications =
            from gpa in groupProblemAccounts
            join customer in linkedCustomers on gpa.CustId equals customer.CustId
            join status in customerStatus on new { customer.CustId, customer.SubAccId }
                equals new { status.CustId, status.SubAccId }
            where status.IsValid(job.SubAccPermission)
            select BuildProblemAccountNotification(job, identity++, gpa, customer);

        return notifications.ToList();
    }

    private ProblemAccountNotification BuildProblemAccountNotification(
        ScheduleJob job,
        int identity,
        GroupProblemAccount groupProblemAccount,
        LinkedCustomer customer)
        => new (
            identity,
            customer.UserId,
            problemAccountService.BuildMessage(
                customer.Language,
                groupProblemAccount.Usernames,
                groupProblemAccount.CreatedDate.ToSimpleDate()),
            customer.IsRealUser,
            job.IsUAT,
            groupProblemAccount.LogIds);
}