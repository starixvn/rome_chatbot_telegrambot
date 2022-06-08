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

            if (_handleUpdateService.BotClients.ContainsKey(accessToken))
            {
                var botClient = _handleUpdateService.BotClients[accessToken];
                await _handleUpdateService.HandleUpdateAsync(botClient, update);

                return Ok();
            }

            return BadRequest();
        }
    }
}
