using System.Net;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.OTP;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OTPController : ControllerBase
{
    private readonly IOTPService otpService;
    private readonly ISettingService settingService;
    private readonly IMessageSender messageSender;
    private readonly IMessageBuilder messageBuilder;
    private const int MaxFailedTimes = 5;

    public OTPController(
        IOTPService otpService,
        ISettingService settingService,
        IMessageSender messageSender,
        IMessageBuilder messageBuilder)
    {
        this.otpService = otpService;
        this.settingService = settingService;
        this.messageSender = messageSender;
        this.messageBuilder = messageBuilder;
    }

    [HttpPost("send")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> SendOTP([FromBody] SendOtpParam param)
    {
        var messenger = await settingService.GetCustBotMessenger(param.CustId, param.SubAccId);
        var isValidMessenger = IsValidMessenger(messenger);
        if (!isValidMessenger)
        {
            return BadRequest();
        }

        var otp = await otpService.GenerateOTP(param.CustId, param.SubAccId);
        var message = messageBuilder.BuildOTPMessage(messenger.Language, otp);
        await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message, FunctionIds.OTP);

        return Ok();
    }

    [HttpPost("verify")]
    [ProducesResponseType(typeof(ActionResult<Result>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Result>> VerifyOTP([FromBody] VerifyOTPParam param)
    {
        var messenger = await settingService.GetCustBotMessenger(param.CustId, param.SubAccId);
        var isValidMessenger = IsValidMessenger(messenger);
        if (!isValidMessenger)
        {
            return BadRequest();
        }

        var result = await otpService.VerifyOTP(param.CustId, param.SubAccId, param.OTP, MaxFailedTimes);
        return Ok(result);
    }

    private static bool IsValidMessenger(CustBotMessenger messenger)
    {
        return messenger != null && messenger.IsFunctionAvailable(FunctionIds.OTP);
    }
}