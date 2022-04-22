using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Telegram.Bot.Types.Enums;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly ICryptography _cryptography;
        private readonly ISettingService _settingService;
        private readonly IBotService _botService;
        private readonly ISenderService _senderService;

        public SettingController(ICryptography cryptography, ISettingService settingService, IBotService botService, ISenderService senderService)
        {
            _cryptography = cryptography;
            _settingService = settingService;
            _botService = botService;
            _senderService = senderService;
        }

        /// <summary>
        /// Generate telegram start URI
        /// </summary>
        /// <response code="200">
        /// https://t.me/LeoVnBot?start=47TfJ2awd8NkARFq4ktqmA==
        /// </response>
        [Route("start-uri"), HttpPost]
        [Produces(MediaTypeNames.Text.Plain)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<string> GenerateStartUri([FromBody] GenerateStartUriParam param)
        {
            var plainKey = $"{param.CustId}:{param.SubAccId}";
            var encryptKey = _cryptography.Encrypt(plainKey);
            var botEntity = await _botService.GetAsync(param.System);
            return $"https://t.me/{botEntity.Name}?start={encryptKey}";
        }

        /// <summary>
        /// Save messenger and customer
        /// </summary>
        [Route("start"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> Start(StartParam param)
        {
            var bot = await _botService.GetAsync(param.MessengerBotId);
            var words = param.Message.Split(' ');
            if (words.Length != 2)
            {
                await _senderService.SendMessage(bot.BotId, param.ChatId, ResourceHelper.Get(nameof(SharedResource.IntroductionMessage), param.Language));
                return Result.CreateErrorResult("Invalid start request");
            }

            var uniqueToken = words[1];
            var plainKey = _cryptography.Decrypt(uniqueToken);
            words = plainKey.Split(':');
            if (words.Length != 2)
            {
                await _senderService.SendMessage(bot.BotId, param.ChatId, ResourceHelper.Get(nameof(SharedResource.IntroductionMessage), param.Language), ParseMode.Html);
                return Result.CreateErrorResult("Invalid start unique token");
            }

            var custId = int.Parse(words[0]);
            var subAccId = int.Parse(words[1]);
            return await _settingService.Start(bot.BotId, param.ChatId, custId, subAccId, param.Language, param.IP);
        }

        /// <summary>
        /// Resend OTP
        /// </summary>
        [Route("resend-otp"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> ResendOtp(ResendOtpParam param)
        {
            return await _settingService.ResendOtp(param.BotId, param.CustId, param.SubAccId);
        }

        /// <summary>
        /// Link with OTP
        /// -1: OTP is expired
        /// -2: Invalid OTP
        /// </summary>
        [Route("link"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> Link(LinkParam param)
        {
            return await _settingService.Link(param.BotId, param.CustId, param.SubAccId, param.Otp, param.IP);
        }

        /// <summary>
        /// Unlink
        /// </summary>
        [Route("unlink"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> Unlink(UnlinkParam param)
        {
            return await _settingService.Unlink(param.BotId, param.CustId, param.SubAccId, param.IP);
        }

        /// <summary>
        /// Get cust bot messenger
        /// </summary>
        [Route("custBotMessenger"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(CustBotMessenger), StatusCodes.Status200OK)]
        public async Task<CustBotMessenger> GetCustBotMessengerAsync(GetCustBotMessengerParam param)
        {
            return await _settingService.GetCustBotMessenger(param.CustId, param.SubAccId);
        }

        [HttpPost("{userId}/function")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> UpdateFunction(int userId, [FromBody] UpdateFunctionParam param)
        {
            return await _settingService.UpdateFunction(userId, param.Function, param.Creator, param.IP);
        }

        [HttpPost("{userId}/language")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> UpdateLanguage(int userId, [FromBody] UpdateLanguageParam param)
        {
            return await _settingService.UpdateLanguage(userId, param.Language, param.IP);
        }
    }
}