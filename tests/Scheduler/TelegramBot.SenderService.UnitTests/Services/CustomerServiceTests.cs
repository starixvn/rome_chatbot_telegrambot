using Fanex.Data.Repository;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Services;


public class CustomerServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly CustomerService customerService;

    public CustomerServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        customerService = new CustomerService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetCustomerStatus_Always_ReturnsCorrectData(IEnumerable<CustomerStatusParam> customers, IEnumerable<CustomerStatus> status)
    {
        // Arrange
        dynamicRepository.FetchAsync<CustomerStatus>(Arg.Any<GetCustomerStatusCriteria>()).Returns(Task.FromResult(status));

        // Act
        var actualResult = await customerService.GetCustomerStatus(customers);

        // Assert
        Assert.Same(status, actualResult);
    }
}