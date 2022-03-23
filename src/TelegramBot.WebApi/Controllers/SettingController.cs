using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Threading.Tasks;
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
        private readonly ICryptography cryptography;
        private readonly ISettingService settingService;
        private readonly IBotService botService;

        public SettingController(ICryptography cryptography, ISettingService settingService, IBotService botService)
        {
            this.cryptography = cryptography;
            this.settingService = settingService;
            this.botService = botService;
        }

        /// <summary>
        /// Generate telegram start URI
        /// </summary>
        /// <response code="200">
        /// https://t.me/LeoVnBot?start=47TfJ2awd8NkARFq4ktqmA==
        /// </response>
        [Route("{custId}/{subAccountId}/start-uri"), HttpPost]
        [Produces(MediaTypeNames.Text.Plain)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<string> GenerateStartUri(int custId, int subAccountId, [FromBody] GenerateStartUriParam param)
        {
            var plainKey = $"{custId}:{subAccountId}";
            var encryptKey = cryptography.Encrypt(plainKey);
            var bot = await botService.GetAsync(param.System, param.IsTest).ConfigureAwait(false);
            return $"https://t.me/{bot.Name}?start={encryptKey}";
        }
    }
}
