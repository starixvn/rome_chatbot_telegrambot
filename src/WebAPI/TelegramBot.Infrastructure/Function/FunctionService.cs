using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Criteria;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Setting
{
    public interface IFunctionService
    {
        Task<IEnumerable<BotFunction>> GetAll();

        Task<IEnumerable<BotFunction>> GetReportFunctions();

        Task<IEnumerable<BotFunction>> GetCommonFunctions();

        Task<BotFunction> GetFunctionByCommand(string command);

        Task<string> GetFunctionName(short functionId, string language);

        Task<string> GetFunctionDescription(short functionId, string language);
    }

    public class FunctionService : IFunctionService
    {
        private IEnumerable<BotFunction> _functions;

        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IResourceService resourceService;

        public FunctionService(IDynamicRepository dynamicRepository, WebAPIConfiguration webAPIConfiguration, IResourceService resourceService)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.resourceService = resourceService;
        }

        public async Task<IEnumerable<BotFunction>> GetAll()
        {
            _functions ??= await dynamicRepository.FetchAsync<BotFunction>(new BotFunctionSelAllCriteria(!webAPIConfiguration.IsProduction));
            return _functions;
        }

        public async Task<IEnumerable<BotFunction>> GetReportFunctions()
            => (await GetAll()).Where(f => f.Type == FunctionTypes.Report);

        public async Task<BotFunction> GetFunctionByCommand(string command)
            => (await GetAll()).FirstOrDefault(f => !string.IsNullOrEmpty(f.Command) && command.IndexOf(f.Command, StringComparison.OrdinalIgnoreCase) == 0);

        public async Task<string> GetFunctionName(short functionId, string language)
            => (await resourceService.GetResource(functionId.ToString(), ResourceType.FunctionName, language))?.Value;

        public async Task<string> GetFunctionDescription(short functionId, string language)
            => (await resourceService.GetResource(functionId.ToString(), ResourceType.FunctionDescription, language))?.Value;

        public async Task<IEnumerable<BotFunction>> GetCommonFunctions()
            => (await GetAll()).Where(f => f.Type == FunctionTypes.Common);
    }
}