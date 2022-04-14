using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using ONEbook.UM.Services;
using System;
using System.Net;
using System.Threading.Tasks;
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

        public AutoReplyUMFilterAttribute(
            IMaintenanceService maintenanceService,
            ISenderService senderService,
            IBuilderService builderService)
        {
            this.maintenanceService = maintenanceService;
            this.senderService = senderService;
            this.builderService = builderService;
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
                        var message = builderService.BuildUnderMaintenanceMessage(umInfo.From, umInfo.To);
                        await senderService.SendMessage(
                            command.MessengerBotId,
                            command.ChatId,
                            message,
                            logToDatabase: false);

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
