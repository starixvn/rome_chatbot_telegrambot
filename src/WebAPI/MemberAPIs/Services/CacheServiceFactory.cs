using Fanex.Caching;
using Fanex.Caching.Memory;
using Fanex.Caching.Redis;
using MemberAPIs.Constants;

namespace MemberAPIs.Services
{
    public interface ICacheServiceFactory
    {
        ICacheService Get(string cacheProvider = CacheProviderConstants.Memory);
    }

    public class CacheServiceFactory : ICacheServiceFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IDictionary<string, ICacheService> cacheServices;

        public CacheServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            cacheServices = new Dictionary<string, ICacheService>();
        }

        public ICacheService Get(string cacheProvider = CacheProviderConstants.Memory)
        {
            if (cacheServices.TryGetValue(cacheProvider, out var existingService))
            {
                return existingService;
            }

            ICache cacheStorage = cacheProvider == CacheProviderConstants.Memory
                ? serviceProvider.GetRequiredService<MemoryCache>()
                : serviceProvider.GetRequiredService<RedisCache>();

            var cacheService = new CacheService(cacheStorage);
            cacheServices[cacheProvider] = cacheService;
            return cacheService;
        }
    }
}