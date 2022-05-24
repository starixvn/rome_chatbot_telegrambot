using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TelegramBot.Infrastructure.Command;
using TelegramBot.Infrastructure.Command.Models;
using TelegramBot.WebApi.Filters;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(AutoReplyUMFilterAttribute))]
    [ServiceFilter(typeof(AccountStatusFilterAttribute))]
    public class CommandController : ControllerBase
    {
        private readonly ICommandService commandService;

        public CommandController(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        [Route("handle/text"), HttpPost]
        public async Task HandleTextMessage(CommandParam param)
        {
            await commandService.HandleUserReplyByText(param.MessengerBotId, param.ChatId, param.Message);
        }

        [Route("handle/callback"), HttpPost]
        public async Task HandleCallbackQuery(CallbackQueryParam param)
        {
            await commandService.HandleUserReplyByButton(
                param.MessengerBotId,
                param.ChatId,
                param.MessageId,
                param.CallbackId,
                JsonConvert.DeserializeObject<CallbackData>(param.CallbackData));
        }
    }
}