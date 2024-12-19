using Microsoft.AspNetCore.Mvc;
using TelegramBot.Modules.AutoTransfer;
using TelegramBot.WebApi.Params.AutoTransfer;
using TelegramBot.Shared.Resources;
using TelegramBot.Modules.Setting.Constants;
using Alpha.API.Client.Models.AutoTransfer;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoTransferController : ControllerBase
    {
        private readonly IAutoTransferHandler autoTransferHandler;

        public AutoTransferController(IAutoTransferHandler autoTransferHandler)
        {
            this.autoTransferHandler = autoTransferHandler;
        }

        [Route("IsAllDayBalanceTransfer"), HttpGet]
        public async Task<bool> IsAllDayBalanceTransfer(int custId)
            => await autoTransferHandler.IsAllDayBalanceTransfer(custId);

        [Route("Monitor"), HttpPost]
        public async Task<AutoTransferMonitoringPageModel> Monitor([FromBody] AutoTransferMonitorParam param)
            => await autoTransferHandler.Monitor(
                param.CustId,
                param.Username,
                param.Status,
                param.SortedBy,
                param.Direction,
                param.LevelType,
                param.Language);

        [Route("Customers"), HttpPost]
        public async Task<AutoTransferCustomersPageModel> Customers([FromBody] AutoTransferCustomersParam param)
            => await autoTransferHandler.Customers(
                param.CustId,
                param.CustLevel,
                param.Language,
                param.Username,
                param.Status,
                param.AutoTransferStatus,
                param.PageIndex,
                param.PageSize);

        [Route("UpdateStatus"), HttpPost]
        public async Task<Alpha.API.Client.Models.Result> UpdateStatus([FromBody] UpdateStatusParam param)
        {
            if (param == null || !param.IsValid())
            {
                return new Alpha.API.Client.Models.Result(ErrorCodes.BadRequest, ResourceHelper.Get(nameof(SharedResource.BadRequest), param.Language));
            }

            if (await autoTransferHandler.IsAllDayBalanceTransfer(param.UplineId))
            {
                return new Alpha.API.Client.Models.Result(ErrorCodes.ADBTChanged, ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), param.Language));
            }

            return await autoTransferHandler.UpdateStatus(
                param.UplineId, param.SubAccId, param.UplineRoleId, param.Language,
                param.CustId, param.Username, param.State, param.Schedule, param.OldTransferOption);
        }

        [Route("UpdateSchedule"), HttpPost]
        public async Task<Alpha.API.Client.Models.Result> UpdateSchedule([FromBody] UpdateScheduleParam param)
        {
            if (param == null || !param.IsValid())
            {
                return new Alpha.API.Client.Models.Result(ErrorCodes.BadRequest, ResourceHelper.Get(nameof(SharedResource.BadRequest), param.Language));
            }

            if (await autoTransferHandler.IsAllDayBalanceTransfer(param.UplineId))
            {
                return new Alpha.API.Client.Models.Result(ErrorCodes.ADBTChanged, ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), param.Language));
            }

            return await autoTransferHandler.UpdateSchedule(
                param.UplineId, param.SubAccId, param.UplineRoleId, param.Language,
                param.CustId, param.Username, param.Schedule, param.OldTransferOption);
        }

        [Route("UpdateMultiple"), HttpPost]
        public async Task<UpdateMultipleAutoTransferResultModel> UpdateMultiple([FromBody] UpdateMultipleParam param)
        {
            if (param == null)
            {
                return new UpdateMultipleAutoTransferResultModel(ErrorCodes.BadRequest, ResourceHelper.Get(nameof(SharedResource.BadRequest), param.Language), param.Language);
            }

            if (await autoTransferHandler.IsAllDayBalanceTransfer(param.UplineId))
            {
                return new UpdateMultipleAutoTransferResultModel(ErrorCodes.ADBTChanged, ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), param.Language), param.Language);
            }

            return await autoTransferHandler.UpdateMultiple(
                param.UplineId, param.SubAccId, param.UplineRoleId, param.Language,
                param.OnIds, param.OffIds, param.OnNames, param.OffNames,
                param.OnSchedule, param.OffSchedule, param.OnStatus, param.OffStatus, param.OldTransferOption);
        }
    }
}