using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class SystemAnnouncementJob : NotificationBaseJob
{

    private readonly IAnnouncementService announcementService;

    public SystemAnnouncementJob(
        IAnnouncementService announcementService,
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.announcementService = announcementService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<SystemNotification>();
        var now = DateTime.Now;
        var announcements = await announcementService.GetAnnouncements(systemSetting.LastQueryTime.AddSeconds(1), now, job.Environment);
        if (announcements.Any())
        {
            var linkedCustomers = await customerService.GetLinkedCustomersByFunction(job.Name, job.Environment);
            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);
                var customers = from linkedCustomer in linkedCustomers
                                join status in customerStatus
                                on new { linkedCustomer.CustId, linkedCustomer.SubAccId } equals new { status.CustId, status.SubAccId }
                                where status.IsValid(job.SubAccPermission)
                                select linkedCustomer;

                foreach (var customer in customers)
                {
                    notifications = await announcementService.FilterAnnouncementsByCustomer(
                        customer,
                        announcements,
                        customer.Language,
                        job.IsUAT);
                }
            }
        }

        var result = await announcementService.Import(notifications, job.Environment, now);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }
}