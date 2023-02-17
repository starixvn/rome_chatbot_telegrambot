using System.Threading.Tasks;
using Fanex.Data.Repository;
using NSubstitute;
using WLMonitored.AccumWLService.Criteria;
using WLMonitored.AccumWLService.Models;
using WLMonitored.AccumWLService.Services;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Services;

public class TicketDetailServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly ITicketDetailService ticketDetailService;

    public TicketDetailServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        ticketDetailService = new TicketDetailService(dynamicRepository);
    }

    [Fact]
    public async Task GetTicketDetails_Always_CallsFetchAsync()
    {
        // Arrange
        const int lastId = 1;
        const int batchNo = 10000;

        // Act
        await ticketDetailService.GetTicketDetails(lastId, batchNo);

        // Assert
        await dynamicRepository.Received().FetchAsync<TicketDetail>(Arg.Is<GetTicketDetailCriteria>(
            criteria => lastId == criteria.LastId
            && batchNo == criteria.BatchNo
        ));
    }
}