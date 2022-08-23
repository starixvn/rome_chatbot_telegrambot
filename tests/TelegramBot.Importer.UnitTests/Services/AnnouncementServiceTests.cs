using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.Announcement;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Repositories;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class AnnouncementServiceTests
{
    private readonly IAnnouncementRepository announcementRepository;
    private readonly IDynamicRepository dynamicRepository;
    private readonly AnnouncementService announcementService;

    public AnnouncementServiceTests()
    {
        announcementRepository = Substitute.For<IAnnouncementRepository>();
        dynamicRepository = Substitute.For<IDynamicRepository>();
        announcementService = new AnnouncementService(announcementRepository, dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetAnnouncements_Always_ReturnsAnnouncements(DateTime fromDate, DateTime toDate, string environment)
    {
        // Arrange
        var criteria = new GetAnnouncementCriteria(fromDate, toDate, environment);

        // Act
        await announcementService.GetAnnouncements(fromDate, toDate, environment);

        // Assert
        await announcementRepository.Received().GetAnnouncements(
            Arg.Is<GetAnnouncementCriteria>(c => c.FromDate == criteria.FromDate && c.ToDate == criteria.ToDate && c.Environment == criteria.Environment));
    }

    [Theory, AutoData]
    public async Task Import_Always_CallsExecuteAsync(IEnumerable<SystemNotification> notifications, string environment, DateTime lastQueryTime)
    {
        // Act
        await announcementService.Import(notifications, environment, lastQueryTime);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ImportAnnouncementCommand>(c => c.Environment == environment && c.LastQueryTime == lastQueryTime));
    }
}