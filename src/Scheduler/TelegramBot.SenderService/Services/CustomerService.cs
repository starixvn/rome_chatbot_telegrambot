using System.Text;
using Fanex.Data.Repository;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerStatus>> GetCustomerStatus(IEnumerable<CustomerStatusParam> customers);
}

public class CustomerService : ICustomerService
{
    private readonly IDynamicRepository dynamicRepository;

    public CustomerService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public Task<IEnumerable<CustomerStatus>> GetCustomerStatus(IEnumerable<CustomerStatusParam> customers)
        => dynamicRepository.FetchAsync<CustomerStatus>(
            new GetCustomerStatusCriteria(BuildCustListParam(customers.Distinct())));

    private static string BuildCustListParam(IEnumerable<CustomerStatusParam> customers)
    {
        var spParams = new StringBuilder();
        spParams.Append("<Root>");

        foreach (var item in customers)
        {
            spParams.Append($"<r id=\"{item.CustId}\" sub=\"{item.SubAccId}\"/>");
        }

        spParams.Append("</Root>");
        return spParams.ToString();
    }
}