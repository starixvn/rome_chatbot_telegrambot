using Alpha.API.Client.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using TelegramBot.Modules.Bot;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting;
using TelegramBot.Shared.Models;
using TelegramBot.WebApi.Params;

namespace TelegramBot.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AccountStatusFilterAttribute : ActionFilterAttribute
    {
        private const string ParamName = "param";
        private readonly ICustomerService customerService;
        private readonly ISettingService settingService;
        private readonly IBotService botService;
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageSender messageSender;
        private readonly Fanex.Logging.ILogger logger;

        public AccountStatusFilterAttribute(
            ICustomerService customerService,
            ISettingService settingService,
            IBotService botService,
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            Fanex.Logging.ILogger logger)
        {
            this.customerService = customerService;
            this.settingService = settingService;
            this.botService = botService;
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.logger = logger;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (context.ActionArguments.ContainsKey(ParamName) && context.ActionArguments[ParamName] is CommandParam param)
            {
                var bot = await botService.GetAsync(param.MessengerBotId);
                var account = await settingService.GetCustBotAccount(param.ChatId, bot.BotId);
                if (account == null)
                {
                    await base.OnActionExecutionAsync(context, next);
                    return;
                }

                var accountStatus = await customerService.GetCustomerStatus(account.CustId, account.SubAccId);
                if (accountStatus == null || accountStatus.IsProtectedMode)
                {
                    await logger.InfoAsync($"Account Status: {JsonConvert.SerializeObject(accountStatus)}");
                    return;
                }

                var messenger = new Messenger(bot.BotId, param.ChatId);
                if (accountStatus.IsLocked)
                {
                    var message = messageBuilder.BuildAccountLockedMessage(account.Language);
                    await messageSender.SendMessage(account.UserId, messenger, message);
                    return;
                }

                if (!accountStatus.IsActive)
                {
                    var message = messageBuilder.BuildAccountClosedMessage(account.Language);
                    await messageSender.SendMessage(account.UserId, messenger, message);
                    return;
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}