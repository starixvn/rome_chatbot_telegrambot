using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.Customers;
using TelegramBot.Importer.Models.Customers;

namespace TelegramBot.Importer.Services;

public interface ICustomerService
{
    Task<Tuple<IEnumerable<LinkedCustomer>, int>> GetYesterdayWLLinkedCustomers(
        DateTime winLossDate,
        string environment);

    Task<IEnumerable<LinkedCustomer>> GetLinkedCustomers(
        string functionKey,
        string custIds,
        string environment);

    Task<IEnumerable<CustomerStatus>> GetCustomerStatus(IEnumerable<CustomerStatusParam> customers);
}

public class CustomerService : ICustomerService
{
    private readonly IDynamicRepository dynamicRepository;

    public CustomerService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<Tuple<IEnumerable<LinkedCustomer>, int>> GetYesterdayWLLinkedCustomers(
        DateTime winLossDate,
        string environment)
    {
        var command = new GetYesterdayWLLinkedCustomerCommand(winLossDate, environment);
        var winloss = await dynamicRepository.FetchAsync<LinkedCustomer>(command);

        return new Tuple<IEnumerable<LinkedCustomer>, int>(winloss, command.MaxUserId);
    } 

    public Task<IEnumerable<LinkedCustomer>> GetLinkedCustomers(
        string functionKey,
        string custIds,
        string environment)
        => dynamicRepository.FetchAsync<LinkedCustomer>(new GetLinkedCustomerCriteria(
            functionKey,
            custIds,
            environment));

    public Task<IEnumerable<CustomerStatus>> GetCustomerStatus(IEnumerable<CustomerStatusParam> customers)
        => dynamicRepository.FetchAsync<CustomerStatus>(
            new GetCustomerStatusCriteria(BuildCustListParam(customers)));

    private static string BuildCustListParam(IEnumerable<CustomerStatusParam> customers)
    {
        var spParams = new StringBuilder();
        spParams.Append("<Root>");

        foreach (var item in customers)
        {
            spParams.Append(item.ToXml());
        }

        spParams.Append("</Root>");
        return spParams.ToString();
    }
}