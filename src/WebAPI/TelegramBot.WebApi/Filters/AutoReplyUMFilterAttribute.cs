using Microsoft.AspNetCore.Mvc.Filters;
using ONEbook.UM.Services;
using System.Net;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AutoReplyUMFilterAttribute : ActionFilterAttribute
    {
        private const string ParamName = "param";
        private readonly IMaintenanceService maintenanceService;
        private readonly IMessageSender messageSender;
        private readonly IMessageBuilder messageBuilder;
        private readonly IBotService botService;

        public AutoReplyUMFilterAttribute(
            IMaintenanceService maintenanceService,
            IMessageSender messageSender,
            IMessageBuilder messageBuilder,
            IBotService botService)
        {
            this.maintenanceService = maintenanceService;
            this.messageSender = messageSender;
            this.messageBuilder = messageBuilder;
            this.botService = botService;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (context.ActionArguments.ContainsKey(ParamName) && context.ActionArguments[ParamName] is CommandParam command)
            {

                var umInfo = await maintenanceService.GetUnderMaintenanceTimeAsync();
                if (umInfo.IsUnderMaintenanceTime)
                {
                    var bot = await botService.GetAsync(command.MessengerBotId);
                    var message = messageBuilder.BuildUnderMaintenanceMessage(umInfo.From, umInfo.To);
                    await messageSender.SendMessage(new Messenger(bot.BotId, command.ChatId), message);
                    await HandleUnderMaintenance(context.HttpContext, message);

                    return;
                }

            }

            await base.OnActionExecutionAsync(context, next);
        }

        public static Task HandleUnderMaintenance(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            return context.Response.WriteAsync(message);
        }
    }
}