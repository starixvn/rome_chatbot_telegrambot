using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Setting
{
    public interface IFunctionService
    {
        Task<IEnumerable<Function>> GetAll();

        Task<IEnumerable<Function>> GetReportFunctions();
    }

    public class FunctionService : IFunctionService
    {
        private IEnumerable<Function> _functions;

        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;

        public FunctionService(IDynamicRepository dynamicRepository, WebAPIConfiguration webAPIConfiguration)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<IEnumerable<Function>> GetAll()
        {
            if (_functions == null)
            {
                _functions = await dynamicRepository.FetchAsync<Function>(new BotFunctionSelAllCriteria(!webAPIConfiguration.IsProduction));
            }

            return _functions;
        }

        public async Task<IEnumerable<Function>> GetReportFunctions()
            => (await GetAll()).Where(f => f.Type == FunctionTypes.Report);
    }
}