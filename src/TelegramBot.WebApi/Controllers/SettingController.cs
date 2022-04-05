using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Threading.Tasks;
using TelegramBot.Client.Models;
using TelegramBot.Common.Services;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Setting;
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

        public SettingController(ICryptography cryptography, ISettingService settingService, IBotService botService)
        {
            _cryptography = cryptography;
            _settingService = settingService;
            _botService = botService;
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
        public async Task<string> GenerateStartUriAsync([FromBody] GenerateStartUriParam param)
        {
            var plainKey = $"{param.CustId}:{param.SubAccId}";
            var encryptKey = _cryptography.Encrypt(plainKey);
            var botEntity = await _botService.GetAsync(param.System, param.IsTest).ConfigureAwait(false);
            return $"https://t.me/{botEntity.Name}?start={encryptKey}";
        }

        /// <summary>
        /// Save messenger and customer
        /// </summary>
        [Route("start"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> StartAsync(StartParam param)
        {
            var words = param.Message.Split(' ');
            if (words.Length != 2)
            {
                return Result.CreateErrorResult("Invalid start request");
            }

            var uniqueToken = words[1];
            var plainKey = _cryptography.Decrypt(uniqueToken);
            words = plainKey.Split(':');
            if (words.Length != 2)
            {
                return Result.CreateErrorResult("Invalid start unique token");
            }

            var custId = int.Parse(words[0]);
            var subAccId = int.Parse(words[1]);
            return await _settingService.StartAsync(param.MessengerBotId, param.ChatId, custId, subAccId, param.MessengerName, param.Language, param.IP).ConfigureAwait(false);
        }

        /// <summary>
        /// Resend OTP
        /// </summary>
        [Route("resend-otp"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> ResendOtpAsync(ResendOtpParam param)
        {
            return await _settingService.ResendOtpAsync(param.BotId, param.CustId, param.SubAccId, param.IsTest);
        }

        /// <summary>
        /// Link with OTP
        /// -1: OTP is expired
        /// -2: Invalid OTP
        /// </summary>
        [Route("link"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> LinkAsync(LinkParam param)
        {
            return await _settingService.LinkAsync(param.BotId, param.CustId, param.SubAccId, param.Otp, param.IP, param.Reset);
        }

        /// <summary>
        /// Unlink
        /// </summary>
        [Route("unlink"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> UnlinkAsync(UnlinkParam param)
        {
            return await _settingService.UnlinkAsync(param.BotId, param.CustId, param.SubAccId, param.IP);
        }

        /// <summary>
        /// Get cust bot messenger
        /// </summary>
        [Route("custBotMessenger"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(CustBotMessenger), StatusCodes.Status200OK)]
        public async Task<CustBotMessenger> GetCustBotMessengerAsync(GetCustBotMessengerParam param)
        {
            return await _settingService.GetCustBotMessengerAsync(param.CustId, param.SubAccId, param.IsTest);
        }
    }
}
