using Microsoft.AspNetCore.Mvc;
using TelegramBot.Modules.AutoTransfer;
using TelegramBot.Modules.AutoTransfer.Models;
using TelegramBot.Shared.Models;
using TelegramBot.WebApi.Params.AutoTransfer;
using TelegramBot.Shared.Resources;
using TelegramBot.Modules.Setting.Constants;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoTransferController : ControllerBase
    {
        private readonly IAutoTransferService _autoTransferService;

        public AutoTransferController(IAutoTransferService autoTransferService)
        {
            _autoTransferService = autoTransferService;
        }

        [Route("IsAllDayBalanceTransfer"), HttpGet]
        public async Task<bool> IsAllDayBalanceTransfer(int custId)
            => await _autoTransferService.IsAllDayBalanceTransfer(custId);

        [Route("Monitor"), HttpPost]
        public async Task<AutoTransferMonitoringPageModel> Monitor([FromBody] AutoTransferMonitorParam param)
            => await _autoTransferService.Monitor(
                param.CustId, 
                param.Username, 
                param.Status, 
                param.SortedBy, 
                param.Direction, 
                param.Language);

        [Route("Customers"), HttpPost]
        public async Task<AutoTransferCustomersPageModel> Customers([FromBody] AutoTransferCustomersParam param)
            => await _autoTransferService.Customers(
                param.CustId, 
                param.CustLevel, 
                param.Language, 
                param.Username, 
                param.Status, 
                param.AutoTransferStatus, 
                param.PageIndex, 
                param.PageSize);

        [Route("UpdateStatus"), HttpPost]
        public async Task<Result> UpdateStatus([FromBody] UpdateStatusParam param) {
            if (param == null || !param.IsValid())
            {
                  return new Result(ErrorCodes.BadRequest, ResourceHelper.Get(nameof(SharedResource.BadRequest), param.Language));
            }

            if (await _autoTransferService.IsAllDayBalanceTransfer(param.UplineId))
            {
                return new Result(ErrorCodes.ADBTChanged, ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), param.Language));
            }
            
            return await _autoTransferService.UpdateStatus(
                param.UplineId, param.SubAccId, param.UplineRoleId, param.Language,
                param.CustId, param.Username, param.State, param.Schedule, param.OldTransferOption);
        }

        [Route("UpdateSchedule"), HttpPost]
        public async Task<Result> UpdateSchedule([FromBody] UpdateScheduleParam param) {
            if (param == null || !param.IsValid())
            {
                  return new Result(ErrorCodes.BadRequest, ResourceHelper.Get(nameof(SharedResource.BadRequest), param.Language));
            }

            if (await _autoTransferService.IsAllDayBalanceTransfer(param.UplineId))
            {
                return new Result(ErrorCodes.ADBTChanged, ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), param.Language));
            }
            
            return await _autoTransferService.UpdateSchedule(
                param.UplineId, param.SubAccId, param.UplineRoleId, param.Language,
                param.CustId, param.Username, param.Schedule, param.OldTransferOption);
        }

        [Route("UpdateMultiple"), HttpPost]
        public async Task<UpdateMultipleAutoTransferResultModel> UpdateMultiple([FromBody] UpdateMultipleParam param) {
            if (param == null)
            {
                return new UpdateMultipleAutoTransferResultModel(ErrorCodes.BadRequest, ResourceHelper.Get(nameof(SharedResource.BadRequest), param.Language), param.Language);
            }

            if (await _autoTransferService.IsAllDayBalanceTransfer(param.UplineId))
            {
                return new UpdateMultipleAutoTransferResultModel(ErrorCodes.ADBTChanged, ResourceHelper.Get(nameof(SharedResource.ADBTChangeError), param.Language), param.Language);
            }
            
            return await _autoTransferService.UpdateMultiple(
                param.UplineId, param.SubAccId, param.UplineRoleId, param.Language,
                param.OnIds, param.OffIds, param.OnNames, param.OffNames,
                param.OnSchedule, param.OffSchedule, param.OnStatus, param.OffStatus, param.OldTransferOption);
        }
    }
}