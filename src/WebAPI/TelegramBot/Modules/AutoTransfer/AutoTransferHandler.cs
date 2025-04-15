using TelegramBot.Shared.Constants;
using Alpha.API.Client.Services;
using Alpha.API.Client.Models.AutoTransfer;

namespace TelegramBot.Modules.AutoTransfer
{
    public interface IAutoTransferHandler
    {
        Task<bool> IsAllDayBalanceTransfer(int custId);
        Task<AutoTransferMonitoringPageModel> Monitor(int custId, string username, string status, int sortedBy, int direction, byte levelType, string language = LanguageCodes.English);
        Task<AutoTransferCustomersPageModel> Customers(int custId, int custLevel, string language, string username, string status, string autoTransferStatus, int pageIndex, int pageSize);
        Task<Alpha.API.Client.Models.Result> UpdateStatus(
            int uplineId, int subAccId, int uplineRoleId, string language,
            int custId, string username, bool state, string schedule, string oldTransferOption);

        Task<Alpha.API.Client.Models.Result> UpdateSchedule(
            int uplineId, int subAccId, int uplineRoleId, string language,
            int custId, string username, string schedule, string oldTransferOption);

        Task<UpdateMultipleAutoTransferResultModel> UpdateMultiple(
            int uplineId, int subAccId, int uplineRoleId, string language,
            string onIds, string offIds, string onNames, string offNames,
            string onSchedule, string offSchedule, bool onStatus, bool offStatus, string oldTransferOption);
    }

    public class AutoTransferHandler : IAutoTransferHandler
    {
        private readonly IAutoTransferService autoTransferService;

        public AutoTransferHandler(IAutoTransferService autoTransferService)
        {
            this.autoTransferService = autoTransferService;
        }

        public Task<bool> IsAllDayBalanceTransfer(int custId)
            => autoTransferService.IsAllDayBalanceTransfer(custId);

        public Task<AutoTransferMonitoringPageModel> Monitor(int custId, string username, string status, int sortedBy, int direction, byte levelType, string language = LanguageCodes.English)
            => autoTransferService.Monitor(custId, username, status, sortedBy, direction, levelType, language);

        public Task<AutoTransferCustomersPageModel> Customers(
            int custId,
            int custLevel,
            string language,
            string username,
            string status,
            string autoTransferStatus,
            int pageIndex,
            int pageSize)
            => autoTransferService.Customers(custId, custLevel, language, username, status, autoTransferStatus, pageIndex, pageSize);

        public Task<Alpha.API.Client.Models.Result> UpdateStatus(
            int uplineId, int subAccId, int uplineRoleId, string language,
            int custId, string username, bool state, string schedule, string oldTransferOption)
            => autoTransferService.UpdateStatus(uplineId, subAccId, uplineRoleId, language,
                custId, username, state, schedule, oldTransferOption);

        public Task<Alpha.API.Client.Models.Result> UpdateSchedule(
            int uplineId, int subAccId, int uplineRoleId, string language,
            int custId, string username, string schedule, string oldTransferOption)
            => autoTransferService.UpdateSchedule(uplineId, subAccId, uplineRoleId, language,
                custId, username, schedule, oldTransferOption);

        public Task<UpdateMultipleAutoTransferResultModel> UpdateMultiple(
            int uplineId, int subAccId, int uplineRoleId, string language,
            string onIds, string offIds, string onNames, string offNames,
            string onSchedule, string offSchedule, bool onStatus, bool offStatus, string oldTransferOption)
            => autoTransferService.UpdateMultiple(uplineId, subAccId, uplineRoleId, language,
                onIds, offIds, onNames, offNames,
                onSchedule, offSchedule, onStatus, offStatus, oldTransferOption);
    }
}