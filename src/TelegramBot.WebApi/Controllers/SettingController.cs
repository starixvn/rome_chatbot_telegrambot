using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Threading.Tasks;
using TelegramBot.Common.Models;
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
        public async Task<string> GenerateStartUri([FromBody] GenerateStartUriParam param)
        {
            var plainKey = $"{param.CustId}:{param.SubAccId}";
            var encryptKey = _cryptography.Encrypt(plainKey);
            var bot = await _botService.GetAsync(param.System, param.IsTest).ConfigureAwait(false);
            return $"https://t.me/{bot.Name}?start={encryptKey}";
        }

        /// <summary>
        /// Save messenger and customer
        /// </summary>
        [Route("start"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> StartAsync(StartParam param)
        {
            var uniqueToken = param.Message.Split(' ')[1];
            var plainKey = _cryptography.Decrypt(uniqueToken);
            var words = plainKey.Split(':');
            var custId = int.Parse(words[0]);
            var subAccId = int.Parse(words[1]);
            return await _settingService.StartAsync(param.MessengerBotId, param.ChatId, custId, subAccId, param.MessengerName, param.Language);
        }

        /// <summary>
        /// Link with OTP
        /// </summary>
        [Route("link"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> LinkAsync(LinkParam param)
        {
            return await _settingService.LinkAsync(param.BotId, param.CustId, param.SubAccId, param.Otp);
        }

        /// <summary>
        /// Unlink
        /// </summary>
        [Route("unlink"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        public async Task<Result> UnlinkAsync(UnlinkParam param)
        {
            return await _settingService.UnlinkAsync(param.BotId, param.CustId, param.SubAccId);
        }
    }
}
