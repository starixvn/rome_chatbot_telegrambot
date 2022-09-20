using Fanex.Caching;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Criteria;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Setting
{
    public interface IFunctionService
    {
        Task<IEnumerable<BotFunction>> GetAll();

        Task<IEnumerable<BotFunction>> GetReportAndCommonFunctions();

        Task<string> GetFunctionName(short functionId, string language);

        Task<string> GetFunctionDescription(short functionId, string language);
    }

    public class FunctionService : IFunctionService
    {
        private const string AllFunctionsCacheKey = "TelegramBot.Infrastructure.Function.FunctionService.AllFunctions";
        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IResourceService resourceService;
        private readonly ICacheService cacheService;

        public FunctionService(
            IDynamicRepository dynamicRepository,
            WebAPIConfiguration webAPIConfiguration,
            IResourceService resourceService,
            ICacheServiceFactory cacheServiceFactory)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.resourceService = resourceService;
            cacheService = cacheServiceFactory.Get();
        }

        public Task<IEnumerable<BotFunction>> GetAll()
            => cacheService.GetOrSetAsync(
                AllFunctionsCacheKey,
                () => dynamicRepository.FetchAsync<BotFunction>(new BotFunctionSelAllCriteria(!webAPIConfiguration.IsProduction)),
                new CacheItemOptions().SetAbsoluteExpiration(new TimeSpan(1, 0, 0)));

        public async Task<IEnumerable<BotFunction>> GetReportAndCommonFunctions()
            => (await GetAll()).Where(f => f.Type == FunctionTypes.Report || f.Type == FunctionTypes.Common);

        public async Task<string> GetFunctionName(short functionId, string language)
            => (await resourceService.GetResource(functionId.ToString(), ResourceType.FunctionName, language))?.Value;

        public async Task<string> GetFunctionDescription(short functionId, string language)
            => (await resourceService.GetResource(functionId.ToString(), ResourceType.FunctionDescription, language))?.Value;
    }
}