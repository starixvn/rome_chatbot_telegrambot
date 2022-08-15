using Fanex.Caching;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria;
using TelegramBot.Importer.Models;

namespace TelegramBot.Importer.Services;

public interface IProductService
{
    Task<IEnumerable<byte>> GetOfferedProductIdsByCustomer(int custId);
}

public class ProductService : IProductService
{
    private const string OfferedProductCacheKey = "TelegramBot.Importer.Services.ProductService.OfferedProduct";
    private readonly ICacheService cacheService;
    private readonly IDynamicRepository dynamicRepository;

    public ProductService(ICacheService cacheService, IDynamicRepository dynamicRepository)
    {
        this.cacheService = cacheService;
        this.dynamicRepository = dynamicRepository;
    }

    public async Task<IEnumerable<byte>> GetOfferedProductIdsByCustomer(int custId)
    {
        var products = await cacheService.GetOrSetAsync(
            $"{OfferedProductCacheKey}.{custId}",
            () => dynamicRepository.FetchAsync<Product>(new GetOfferedProductCriteria(custId)),
            new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));

        return products != null ? products.Where(p => !p.IsHidden).Select(p => (byte)p.Id) : Enumerable.Empty<byte>();
    }
}