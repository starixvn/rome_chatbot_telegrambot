using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.Announment;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Repositories;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class AnnouncementServiceTests
{
    private readonly IAnnouncementRepository announcementRepository;
    private readonly IProductService productService;
    private readonly ICustomerService customerService;
    private readonly IDynamicRepository dynamicRepository;
    private readonly AnnouncementService announcementService;

    public AnnouncementServiceTests()
    {
        announcementRepository = Substitute.For<IAnnouncementRepository>();
        productService = Substitute.For<IProductService>();
        customerService = Substitute.For<ICustomerService>();
        dynamicRepository = Substitute.For<IDynamicRepository>();
        announcementService = new AnnouncementService(announcementRepository, productService, customerService, dynamicRepository);
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
}