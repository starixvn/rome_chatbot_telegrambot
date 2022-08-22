using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.Announment;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Repositories;

namespace TelegramBot.Importer.Services;

public interface IAnnouncementService
{
    Task<IEnumerable<Announcement>> GetAnnouncements(DateTime fromDate, DateTime toDate, string environment);

    Task<Result> Import(IEnumerable<SystemNotification> notifications, string environment, DateTime lastQueryTime);
}

public class AnnouncementService : NotificationServiceBase, IAnnouncementService
{

    private readonly IAnnouncementRepository announcementRepository;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        IDynamicRepository dynamicRepository)
        : base(dynamicRepository)
    {
        this.announcementRepository = announcementRepository;
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncements(DateTime fromDate, DateTime toDate, string environment)
        => await announcementRepository.GetAnnouncements(new GetAnnouncementCriteria(fromDate, toDate, environment));

    public Task<Result> Import(IEnumerable<SystemNotification> notifications, string environment, DateTime lastQueryTime)
        => ExecuteNonQuery(new ImportAnnouncementCommand(BuildXmlData(notifications), environment, lastQueryTime));
}