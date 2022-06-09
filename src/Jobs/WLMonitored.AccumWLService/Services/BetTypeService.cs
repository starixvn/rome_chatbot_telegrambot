using Fanex.Caching;
using Fanex.Data.Repository;
using WLMonitored.AccumWLService.Criteria;
using WLMonitored.AccumWLService.Models;

namespace WLMonitored.AccumWLService.Services;

public interface IBetTypeService
{
    Task<Tuple<IDictionary<short, bool>, IDictionary<short, bool>>> GetBetTypes();

    Task<IDictionary<short, bool>> GetCreditBetTypes();

    Task<IDictionary<short, bool>> GetDepositBetTypes();
}

public sealed class BetTypeService : IBetTypeService
{
    private const string Credit = "Credit";
    private const string Deposit = "Deposit";
    private const string BetTypesCacheKey = "WLMonitored.AccumWLService.OfferedBetTypes";
    private const int BetTypesCacheInMinutes = 10;

    private readonly IDynamicRepository dynamicRepository;
    private readonly ICacheService cacheService;

    public BetTypeService(IDynamicRepository dynamicRepository, ICacheService cacheService)
    {
        this.dynamicRepository = dynamicRepository;
        this.cacheService = cacheService;
    }

    public async Task<Tuple<IDictionary<short, bool>, IDictionary<short, bool>>> GetBetTypes()
    {
        var creditBetTypesTask = GetCreditBetTypes();
        var depositBetTypesTask = GetDepositBetTypes();
        await Task.WhenAll(creditBetTypesTask, depositBetTypesTask);

        var creditBetTypes = await creditBetTypesTask;
        var depositBetTypes = await depositBetTypesTask;

        return new Tuple<IDictionary<short, bool>, IDictionary<short, bool>>(creditBetTypes, depositBetTypes);
    }

    public Task<IDictionary<short, bool>> GetCreditBetTypes()
        => GetBetTypesFromCache(Credit);

    public Task<IDictionary<short, bool>> GetDepositBetTypes()
        => GetBetTypesFromCache(Deposit);

    private Task<IDictionary<short, bool>> GetBetTypesFromCache(string customerType)
        => cacheService.GetOrSetAsync(
            $"{BetTypesCacheKey}.{customerType}",
            () => GetBetTypes(customerType),
            new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(BetTypesCacheInMinutes))
        );

    private async Task<IDictionary<short, bool>> GetBetTypes(string customerType)
    {
        var betTypes = await dynamicRepository.FetchAsync<BetTypeEntity>(new GetBetTypeCriteria(customerType));

        return betTypes.ToDictionary(kvp => kvp.BetType, _ => true);
    }
}