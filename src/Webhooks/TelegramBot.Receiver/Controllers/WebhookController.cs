using Fanex.Logging;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.Controllers
{
    public class WebhookController : Controller
    {
        private readonly IHandleUpdateService _handleUpdateService;

        public WebhookController(IHandleUpdateService handleUpdateService)
        {
            _handleUpdateService = handleUpdateService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(string accessToken, [FromBody] Update update)
        {
            Logger.Log.Info(accessToken);
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
