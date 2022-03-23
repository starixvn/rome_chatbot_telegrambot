using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver.Controllers
{
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
        {
            await handleUpdateService.HandleUpdateAsync(null, update);
            return Ok();
        }
    }
}
