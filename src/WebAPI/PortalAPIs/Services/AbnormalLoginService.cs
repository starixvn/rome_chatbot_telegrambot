using Fanex.Data.Repository;
using PortalAPIs.Criteria;
using PortalAPIs.Models;

namespace PortalAPIs.Services
{
    public interface IAbnormalLoginService
    {
        Task<bool> AlertAbnormalLogin(IEnumerable<AbnormalLoginModel> trans);
    }

    public class AbnormalLoginService : IAbnormalLoginService
    {
        private readonly IDynamicRepository dynamicRepository;
        protected readonly ILoggerService loggerService;

        public AbnormalLoginService(IDynamicRepository dynamicRepository, ILoggerService loggerService)
        {
            this.dynamicRepository = dynamicRepository;
            this.loggerService = loggerService;
        }

        public async Task<bool> AlertAbnormalLogin(IEnumerable<AbnormalLoginModel> trans)
        {
            var criteria = new AddAbnormalLoginCriteria(trans);

            await dynamicRepository.ExecuteAsync(criteria);

            if (criteria.ErrCode != 0)
            {
                await loggerService.Error(new Exception(criteria.ErrMsg.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(criteria.ExitsTransIDs))
            {
                await loggerService.Info($"Insert duplicate Ids: {criteria.ExitsTransIDs}");
            }

            return true;
        }
    }
}