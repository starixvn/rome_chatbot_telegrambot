using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Criteria;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Setting
{
    public interface IFunctionService
    {
        Task<IEnumerable<BotFunction>> GetAll();

        Task<IEnumerable<BotFunction>> GetReportFunctions();

        Task<BotFunction> GetFunctionByCommand(string command);
    }

    public class FunctionService : IFunctionService
    {
        private IEnumerable<BotFunction> _functions;

        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;

        public FunctionService(IDynamicRepository dynamicRepository, WebAPIConfiguration webAPIConfiguration)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<IEnumerable<BotFunction>> GetAll()
        {
            if (_functions == null)
            {
                _functions = await dynamicRepository.FetchAsync<BotFunction>(new BotFunctionSelAllCriteria(!webAPIConfiguration.IsProduction));
            }

            return _functions;
        }

        public async Task<IEnumerable<BotFunction>> GetReportFunctions()
            => (await GetAll()).Where(f => f.Type == FunctionTypes.Report);

        public async Task<BotFunction> GetFunctionByCommand(string command)
            => (await GetAll()).FirstOrDefault(f => !string.IsNullOrEmpty(f.Command) && command.IndexOf(f.Command, StringComparison.OrdinalIgnoreCase) == 0);

    }
}