using Fanex.Caching;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Resource.Criteria;
using TelegramBot.Infrastructure.Resource.Models;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Resource
{
    public interface IResourceService
    {
        Task<string> GetActionDescription(short actionId, string language);

        Task<BotResource> GetResource(string key, string type, string language);
    }

    public class ResourceService : IResourceService
    {
        private const string ResourceCacheKey = "TelegramBot.Infrastructure.Resource.ResourceService";
        private readonly ICacheService cacheService;
        private readonly IDynamicRepository dynamicRepository;

        public ResourceService(ICacheServiceFactory cacheServiceFactory, IDynamicRepository dynamicRepository)
        {
            cacheService = cacheServiceFactory.Get();
            this.dynamicRepository = dynamicRepository;
        }

        public async Task<string> GetActionDescription(short actionId, string language)
            => (await GetResource(actionId.ToString(), ResourceType.ActionDescription, language))?.Value;

        public async Task<BotResource> GetResource(string key, string type, string language)
        {
            var resources = await GetResources(key, type);
            return resources.FirstOrDefault(r => r.Language == language)
                 ?? resources.FirstOrDefault(r => r.Language == LanguageCodes.English);
        }

        private async Task<IEnumerable<BotResource>> GetResources(string key, string type)
            => await cacheService.GetOrSetAsync(
                $"{ResourceCacheKey}.{type}.{key}",
                () => dynamicRepository.FetchAsync<BotResource>(new BotResourceSelCriteria(type, key)),
                new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1))).ConfigureAwait(false);
    }
}