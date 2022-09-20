using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria;
using TelegramBot.Importer.Criteria.Customers;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

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
    public async Task GetYesterdayWinLossLinkedCustomers_Always_ReturnsCorrectData(DateTime winLossDate, string environment, IEnumerable<LinkedCustomer> customers)
    {
        // Arrange
        var command = new GetYesterdayWinLossLinkedCustomerCommand(winLossDate, environment);
        dynamicRepository.FetchAsync<LinkedCustomer>(Arg.Any<GetYesterdayWinLossLinkedCustomerCommand>()).Returns(Task.FromResult(customers));
        var expectedResult = new Tuple<IEnumerable<LinkedCustomer>, int>(customers, command.MaxUserId);

        // Act
        var result = await customerService.GetYesterdayWinLossLinkedCustomers(winLossDate, environment);

        // Assert
        Assert.True(expectedResult.Item1.Equals(result.Item1) && result.Item2.Equals(expectedResult.Item2));
    }

    [Theory, AutoData]
    public async Task GetLinkedCustomers_Always_ReturnsLinkedCustomers(string functionKey, string custIds, string environment, IEnumerable<LinkedCustomer> customers)
    {
        // Arrange
        dynamicRepository.FetchAsync<LinkedCustomer>(Arg.Any<GetLinkedCustomerCriteria>()).Returns(Task.FromResult(customers));

        // Act
        var result = await customerService.GetLinkedCustomers(functionKey, custIds, environment);

        // Assert
        Assert.Same(customers, result);
    }

    [Theory, AutoData]
    public async Task GetCustomerStatus_Always_ReturnsCustomerStatus(IEnumerable<CustomerStatusParam> customers, IEnumerable<CustomerStatus> status)
    {
        // Arrange
        dynamicRepository.FetchAsync<CustomerStatus>(Arg.Any<GetCustomerStatusCriteria>()).Returns(Task.FromResult(status));

        // Act
        var result = await customerService.GetCustomerStatus(customers);

        // Assert
        Assert.Same(status, result);
    }

    [Theory, AutoData]
    public async Task GetCustomerInfo_Always_ReturnsCustomerInfo(int custId, Customer customer)
    {
        // Arrange
        dynamicRepository.GetAsync<Customer>(Arg.Any<GetCustomerInfoCriteria>()).Returns(Task.FromResult(customer));

        // Act
        var result = await customerService.GetCustomerInfo(custId);

        // Assert
        Assert.Same(customer, result);
    }

    [Theory, AutoData]
    public async Task GetLinkedCustomersByFunction_Always_ReturnsCustomerInfo(string functionKey, string environment, IEnumerable<LinkedCustomer> customers)
    {
        // Arrange
        dynamicRepository.FetchAsync<LinkedCustomer>(Arg.Any<GetLinkedCustomerByFunctionCriteria>()).Returns(Task.FromResult(customers));

        // Act
        var result = await customerService.GetLinkedCustomersByFunction(functionKey, environment);

        // Assert
        Assert.Same(customers, result);
    }
}