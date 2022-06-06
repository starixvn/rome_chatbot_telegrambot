using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.Controllers
{
    public class WebhookController : Controller
    {
        private readonly IHandleUpdateService _handleUpdateService;
        private readonly Fanex.Logging.ILogger _logger;

        public WebhookController(IHandleUpdateService handleUpdateService, Fanex.Logging.ILogger logger)
        {
            _handleUpdateService = handleUpdateService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(string accessToken, [FromBody] Update update)
        {
            await _logger.InfoAsync(accessToken);
            await _logger.InfoAsync(JsonConvert.SerializeObject(update));
            var id = _handleUpdateService.BotEntities.FindIndex(x => x.AccessToken == accessToken);
            var botClient = _handleUpdateService.BotClients[id];
            await _handleUpdateService.HandleUpdateAsync(botClient, update);
            return Ok();
        }
    }
}
