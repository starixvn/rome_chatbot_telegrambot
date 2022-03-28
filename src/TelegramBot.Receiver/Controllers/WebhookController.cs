using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
            var id = _handleUpdateService.BotEntities.FindIndex(x => x.AccessToken == accessToken);
            var botClient = _handleUpdateService.BotClients[id];
            await _handleUpdateService.HandleUpdateAsync(botClient, update);
            return Ok();
        }
    }
}
