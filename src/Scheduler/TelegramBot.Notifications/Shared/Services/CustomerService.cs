using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Notifications.Shared.Criteria;
using TelegramBot.Notifications.Shared.Models;
using TelegramBot.Notifications.Shared.Params;

namespace TelegramBot.Notifications.Shared.Services;

public interface ICustomerService
{
    Task<IEnumerable<LinkedCustomer>> GetLinkedCustomers(
        string system,
        bool botStatus,
        bool isBotTest,
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

    public Task<IEnumerable<LinkedCustomer>> GetLinkedCustomers(
        string system,
        bool botStatus,
        bool isBotTest,
        string functionKey,
        string custIds,
        string environment)
        => dynamicRepository.FetchAsync<LinkedCustomer>(new GetCustomerBotCriteria(
            system,
            botStatus,
            isBotTest,
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