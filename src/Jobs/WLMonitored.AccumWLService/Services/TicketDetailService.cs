using Fanex.Data.Repository;
using WLMonitored.AccumWLService.Criteria;
using WLMonitored.AccumWLService.Models;

namespace WLMonitored.AccumWLService.Services;

public interface ITicketDetailService
{
    Task<IEnumerable<TicketDetail>> GetTicketDetails(long lastId, int batchSize);
}

public sealed class TicketDetailService : ITicketDetailService
{
    private readonly IDynamicRepository dynamicRepository;

    public TicketDetailService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public Task<IEnumerable<TicketDetail>> GetTicketDetails(long lastId, int batchSize)
        => dynamicRepository.FetchAsync<TicketDetail>(new GetTicketDetailCriteria(lastId, batchSize));
}