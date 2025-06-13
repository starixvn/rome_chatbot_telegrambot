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
            var processDataResult = await PreprocessData(trans);

            if (processDataResult.ErrCode != 0)
            {
                await loggerService.Error(new Exception(processDataResult.ErrMsg.ToString()));
                return false;
            }

            return await InsertData(processDataResult.Data);
        }

        public async Task<ResultModel> PreprocessData(IEnumerable<AbnormalLoginModel> trans)
        {
            var criteria = new PrepareAbnormalLoginInsertCriteria(trans);

            await dynamicRepository.ExecuteAsync(criteria);

            return new ResultModel
            {
                Data = criteria.NewBatch,
                ErrCode = criteria.ErrCode,
                ErrMsg = criteria.ErrMsg
            };
        }
   
        public async Task<bool> InsertData(string batch)
        {
            var criteria = new AddAbnormalLoginCriteria(batch);

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