using Fanex.Data.Repository;
using TelegramBot.Modules.Setting.Criteria;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Modules.AutoTransfer.Criteria;
using TelegramBot.Modules.AutoTransfer.Models;
using TelegramBot.Shared.Resources;
using TelegramBot.Shared.Models;
using TelegramBot.Modules.AutoTransfer.Params;

namespace TelegramBot.Modules.AutoTransfer
{
    public interface IAutoTransferService
    {
        Task<bool> IsAllDayBalanceTransfer(int custId);
        Task<string> GetTransferCondition(int custId);
        Task<AutoTransferMonitoringPageModel> Monitor(int custId, string username, string status, int sortedBy, int direction, string language = LanguageCodes.English);
        Task<AutoTransferCustomersPageModel> Customers(int custId, int custLevel, string language, string username, string status, string autoTransferStatus, int pageIndex, int pageSize);
        Task<Result> UpdateStatus(
            int uplineId, int subAccId, int uplineRoleId, string language, 
            int custId, string username, bool state, string schedule, string oldTransferOption);

        Task<Result> UpdateSchedule(
            int uplineId, int subAccId, int uplineRoleId, string language, 
            int custId, string username, string schedule, string oldTransferOption);

        Task<UpdateMultipleAutoTransferResultModel> UpdateMultiple(
            int uplineId, int subAccId, int uplineRoleId, string language, 
            string onIds, string offIds, string onNames, string offNames, 
            string onSchedule, string offSchedule, bool onStatus, bool offStatus, string oldTransferOption);
    }

    public class AutoTransferService : IAutoTransferService
    {
        private const int WarningCode = -1;

        private readonly IDynamicRepository _dynamicRepository;

        public AutoTransferService(IDynamicRepository dynamicRepository)
        {
            _dynamicRepository = dynamicRepository;
        }

        public async Task<bool> IsAllDayBalanceTransfer(int custId) {
            var data = await _dynamicRepository.GetAsync<AllDayBalanceTransferModel>(new GetIsAllDayBalTransferCriteria(custId));
            return data != null && data.IsAllDayBalTransfer;
        }

        public async Task<AutoTransferMonitoringPageModel> Monitor(int custId, string username, string status, int sortedBy, int direction, string language = LanguageCodes.English)
        {
            var isAllDayBalanceTransfer = await IsAllDayBalanceTransfer(custId);

            if(isAllDayBalanceTransfer) {
                return new AutoTransferMonitoringPageModel { 
                    Code = 108,
                    Message = ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), language)
                };
            }

            var siteDB = await _dynamicRepository.GetAsync<SiteDBGroup>(new GetSiteDBConfigCriteria(custId));
           
            var criteria = new GetMonitorCriteria
            {
                SiteDBGroupID = siteDB.SiteDBGroupID.ToString(),
                FromCustId = custId,
                Language = LanguageCodes.LanguageKeys.ContainsKey(language) ?LanguageCodes.LanguageKeys[language] : "E",
                Searchname = string.IsNullOrWhiteSpace(username) ? null : username,
                ViewStatus = status,
                SortBy = sortedBy,
                SortType = direction
            };
            
            var transfers = await _dynamicRepository.FetchAsync<AutoTransferMonitoringModel>(criteria);
            
            return new AutoTransferMonitoringPageModel
            {
                Transfers = transfers,
                TotalItems = criteria.TotalRow,
                PageSize = criteria.PageSize,
                PageIndex = criteria.PageIndex,
                SortedBy = criteria.SortBy,
                Direction = criteria.SortType,

                TotalCancelAll = criteria.TotalCancelAll,
                TotalPending = criteria.TotalPending,
                TotalProcessing = criteria.TotalProcessing,
                TotalFailed = criteria.TotalFailed,
            };
        }
    
        public async Task<AutoTransferCustomersPageModel> Customers(
            int custId, 
            int custLevel, 
            string language, 
            string username, 
            string status, 
            string autoTransferStatus, 
            int pageIndex, 
            int pageSize) {
            var isAllDayBalanceTransfer = await IsAllDayBalanceTransfer(custId);

            if(isAllDayBalanceTransfer) {
                return new AutoTransferCustomersPageModel { 
                    Code = 108,
                    Message = ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), language)
                };
            }

            var siteDB = await _dynamicRepository.GetAsync<SiteDBGroup>(new GetSiteDBConfigCriteria(custId));
            GetCustomersCriteria criteria;

            switch(custLevel) {
                case (int)CustomerLevel.Super:
                    criteria = new GetMastersCriteria
                    {
                        SAgId = custId,
                        PageSize = pageSize,
                        PageIndex = pageIndex,
                        Username = username,
                        CustStatus = GetStatusName(status),
                        AutoTransfer = autoTransferStatus
                    };
                    break;
                case (int)CustomerLevel.Master:
                    criteria = new GetAgentsCriteria
                    {
                        MasterId = custId,
                        PageSize = pageSize,
                        PageIndex = pageIndex,
                        Username = username,
                        CustStatus = GetStatusName(status),
                        AutoTransfer = autoTransferStatus
                    };
                    break;
                default:
                    criteria = new GetMembersCriteria
                    {
                        SiteDBGroupID = siteDB.SiteDBGroupID.ToString(),
                        CustId = custId,
                        PageSize = pageSize,
                        PageIndex = pageIndex,
                        Username = username,
                        CustStatus = GetStatusName(status),
                        AutoTransfer = autoTransferStatus
                    };
                    break;
            }

           var customers = await _dynamicRepository.FetchAsync<AutoTransferCustomerModel>(criteria);
           var transferCondition = await GetTransferCondition(custId);
            customers = customers.OrderBy(c => c.Username).ToList();
            return new AutoTransferCustomersPageModel
            {
               Customers = customers,
               UplineTransferOption = transferCondition,
               UplineRoleId = custLevel,
               PageIndex = pageIndex,
               PageSize = pageSize,
               TotalPages = criteria.TotalPages
            };
        }

        public async Task<string> GetTransferCondition(int custId) {
            var criteria = new GetTransferConditionCriteria(custId);
            await _dynamicRepository.ExecuteAsync(criteria);
            return criteria.TransferOption;
        }

        public async Task<Result> UpdateStatus(
            int uplineId, int subAccId, int uplineRoleId, string language, 
            int custId, string username, bool state, string schedule, string oldTransferOption) {
            var isAutoJobRunning = await GetJobStatus(custId, uplineId, state);

            var criteria = new UpdateStatusCriteria {
                IsAutoJobRunning = isAutoJobRunning,

                FromCustId = uplineId,
                ToCustId = custId,
                AutoTransferSchedule = schedule,
                Language = language,
                Old_TransferOption = oldTransferOption,
                IsAllowAutoTransfer = state,
                IP = string.Empty
            };

            await _dynamicRepository.ExecuteAsync(criteria);

            if(criteria.ErrCode == 0 && !string.IsNullOrEmpty(criteria.ErrMsg)) {
                return new Result(WarningCode, criteria.ErrMsg);
            }

            var message = state 
                ? ResourceHelper.Get(nameof(SharedResource.AutoTransfer_UpdateSuccessOn), language)
                : ResourceHelper.Get(nameof(SharedResource.AutoTransfer_UpdateSuccessOff), language);

            return new Result(criteria.ErrCode, criteria.ErrCode == 0 ? string.Format(message, username) : criteria.ErrMsg);
        }

        public async Task<Result> UpdateSchedule(
            int uplineId, int subAccId, int uplineRoleId, string language, 
            int custId, string username, string schedule, string oldTransferOption) {
            var isAutoJobRunning = await GetJobStatus(custId, uplineId, true);

            var criteria = new UpdateScheduleCriteria {
                IsAutoJobRunning = isAutoJobRunning,
                FromCustId = uplineId,
                ToCustId = custId,
                AutoTransferSchedule = schedule,
                Language = language,
                Old_TransferOption = oldTransferOption,
                IP = string.Empty
            };

            await _dynamicRepository.ExecuteAsync(criteria);

            if(criteria.ErrCode == 0 && !string.IsNullOrEmpty(criteria.ErrMsg)) {
                return new Result(WarningCode, criteria.ErrMsg);
            }

            var message = ResourceHelper.Get(nameof(SharedResource.AutoTransfer_UpdateScheduleSuccess), language);

            return new Result(criteria.ErrCode, criteria.ErrCode == 0 ? string.Format(message, username) : criteria.ErrMsg);
        }

        public async Task<UpdateMultipleAutoTransferResultModel> UpdateMultiple(
            int uplineId, int subAccId, int uplineRoleId, string language, 
            string onIds, string offIds, string onNames, string offNames, 
            string onSchedule, string offSchedule, bool onStatus, bool offStatus, string oldTransferOption) {
            var result = new UpdateMultipleAutoTransferResultModel { Language = language };

            var updateSingleParam = new UpdateSingleParam
            {
                UplineId = uplineId,
                Language = language,
                Status = offStatus,
                SubAccId = subAccId,
                Schedule = offSchedule ?? string.Empty,
                OldTransferOption = oldTransferOption,
                UplineRoleId = uplineRoleId
            };

            await UpdateMultipleAutoTransferToStatusAsync(updateSingleParam, offIds, offNames, result);
            
            updateSingleParam = new UpdateSingleParam
            {
                UplineId = uplineId,
                Language = language,
                Status = onStatus,
                SubAccId = subAccId,
                Schedule = onSchedule ?? string.Empty,
                OldTransferOption = oldTransferOption,
                UplineRoleId = uplineRoleId
            };

            if (string.IsNullOrWhiteSpace(result.StopMessage))
            {
                await UpdateMultipleAutoTransferToStatusAsync(updateSingleParam, onIds, onNames, result);
            }
            
            return result;
        }

        public async Task UpdateMultipleAutoTransferToStatusAsync(
            UpdateSingleParam param,
            string pIds,
            string pNames,
            UpdateMultipleAutoTransferResultModel result)
        {
            var stopCodes = new[] { 105, 108 };
            var ids = (pIds ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var names = (pNames ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var name = names[i].Trim();

                param.CustId =  int.Parse(id);
                param.Username =  name;
                var singleResult = await UpdateStatus(
                    param.UplineId, param.SubAccId, param.UplineRoleId, param.Language, 
                    param.CustId, param.Username, param.Status, param.Schedule, param.OldTransferOption);
            
                if (singleResult.Code == 0)
                {
                    continue;
                }
                
                if (singleResult.Code == WarningCode)
                {
                    result.WarningCusts.Add(name);
                    continue;
                }

                if (stopCodes.Any(s => s == singleResult.Code))
                {
                    result.Code = singleResult.Code;
                    result.StopMessage = singleResult.Message;
                    return;
                }

                var code = singleResult.Code.ToString();
                if (result.ErrMessages.ContainsKey(code))
                {
                    result.ErrCusts[code].Add(name);
                    continue;
                }

                result.ErrMessages.Add(code, singleResult.Message);
                result.ErrCusts.Add(code, new List<string> { name });
            }
        }

        private static string GetStatusName(string status) {
            var s = int.Parse(status);

            switch (s)
            {
                case 1:
                    return "Open";
                case 2:
                    return "Suspended";
                case 3:
                    return "Closed";
                case 4:
                    return "Disabled";
                default:
                    return "All";
            }
        }

        private async Task<bool> GetJobStatus(int custId, int creator, bool state)
        {
            var siteDB = await _dynamicRepository.GetAsync<SiteDBGroup>(new GetSiteDBConfigCriteria(custId));
            var criteria = new GetJobStatusCriteria(creator, custId, state, siteDB.SiteDBGroupID);
            await _dynamicRepository.ExecuteAsync(criteria);

            return criteria.IsAutoJobRunning;
        }
    }
}