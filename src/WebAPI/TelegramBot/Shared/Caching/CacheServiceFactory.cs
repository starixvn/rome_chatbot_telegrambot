using Fanex.Caching;
using Fanex.Caching.Memory;
using Fanex.Caching.Redis;

namespace TelegramBot.Shared.Caching
{
    public static class CacheProvider
    {
        public const string Memory = "memory";
        public const string Redis = "redis";
    }

    public interface ICacheServiceFactory
    {
        ICacheService Get(string cacheProvider = CacheProvider.Memory);
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

        public ICacheService Get(string cacheProvider = CacheProvider.Memory)
        {
            if (cacheServices.ContainsKey(cacheProvider))
            {
                return cacheServices[cacheProvider];
            }

            var cacheStorage = cacheProvider == CacheProvider.Memory
                ? (ICache)serviceProvider.GetService(typeof(MemoryCache))
                : (ICache)serviceProvider.GetService(typeof(RedisCache));

            var cacheService = new CacheService(cacheStorage);
            cacheServices[cacheProvider] = cacheService;
            return cacheService;
        }
    }
}