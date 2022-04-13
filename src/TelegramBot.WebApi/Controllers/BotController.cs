using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using TelegramBot.Client.Models;
using TelegramBot.Infrastructure.Bot;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotService _botService;

        public BotController(IBotService botService)
        {
            _botService = botService;
        }

        /// <summary>
        /// Get all bots
        /// </summary>
        /// <response code="200">
        /// [{"BotId":1,"Name":"LeoVnBot","System":"IBC","AccessToken":"5237914512:AAFlfL-a8mJZMLpEuyDE7QDB3d8Hn8aV7lY"}]
        /// </response>
        [Route("all"), HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<BotEntity>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<BotEntity>> GetAllAsync()
        {
            return await _botService.GetAllAsync().ConfigureAwait(false);
        }
    }
}
