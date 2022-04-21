using Microsoft.AspNetCore.Mvc.Filters;
using ONEbook.UM.Services;
using System.Net;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AutoReplyUMFilterAttribute : ActionFilterAttribute
    {
        private const string ParamName = "param";
        private readonly IMaintenanceService maintenanceService;
        private readonly ISenderService senderService;
        private readonly IBuilderService builderService;
        private readonly IBotService botService;

        public AutoReplyUMFilterAttribute(
            IMaintenanceService maintenanceService,
            ISenderService senderService,
            IBuilderService builderService,
            IBotService botService)
        {
            this.maintenanceService = maintenanceService;
            this.senderService = senderService;
            this.builderService = builderService;
            this.botService = botService;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (context.ActionArguments.ContainsKey(ParamName))
            {
                if (context.ActionArguments[ParamName] is CommandParam command)
                {
                    var umInfo = await maintenanceService.GetUnderMaintenanceTimeAsync();
                    if (umInfo.IsUnderMaintenanceTime)
                    {
                        var bot = await botService.GetAsync(command.MessengerBotId);
                        var message = builderService.BuildUnderMaintenanceMessage(umInfo.From, umInfo.To);
                        await senderService.SendMessage(bot.BotId, command.ChatId, message);
                        await HandleUnderMaintenance(context.HttpContext, message);

                        return;
                    }
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