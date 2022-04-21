using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Command;
using TelegramBot.WebApi.Filters;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly ICommandService commandService;

        public CommandController(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        //[ServiceFilter(typeof(AutoReplyUMFilterAttribute))]
        [Route("execute"), HttpPost]
        public async Task ExecuteAsync(CommandParam param)
        {
            await commandService.ExecuteAsync(param.MessengerBotId, param.ChatId, param.Message, param.IsCallback);
        }
    }
}