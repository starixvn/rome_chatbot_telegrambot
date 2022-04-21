using Alpha.API.Client.Services;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Command
{
    public interface ICommandService
    {
        Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback);
    }

    public class CommandService : ICommandService
    {
        private readonly IBuilderService builderService;
        private readonly ISenderService senderService;
        private readonly ISettingService settingService;
        private readonly IBotService botService;
        private readonly ICustomerService customerService;
        private readonly IActionService actionService;

        public CommandService(
            IBuilderService builderService,
            ISenderService senderService,
            ISettingService settingService,
            IBotService botService,
            ICustomerService customerService,
            IActionService actionService)
        {
            this.builderService = builderService;
            this.senderService = senderService;
            this.settingService = settingService;
            this.botService = botService;
            this.customerService = customerService;
            this.actionService = actionService;
        }

        public async Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback)
        {
            if (messengerBotId == 0 || chatId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (message.Trim().Equals("/help"))
            {
                await HandleHelpCommand(messengerBotId, chatId);
                return;
            }

            if (message.Trim().Equals("/menu"))
            {
                await HandleMenuCommand(messengerBotId, chatId);
                return;
            }
        }

        private async Task HandleHelpCommand(long messengerBotId, long chatId)
        {
            var bot = await botService.GetAsync(messengerBotId);
            var custBotAccount = await settingService.GetCustBotAccount(chatId, bot.BotId);
            var message = builderService.BuildHelpMessage(
                LanguageCodes.English,
                bot.Name,
                !custBotAccount.IsSubAccount || (await customerService.IsSubAccHasReportPermissionAsync(custBotAccount.CustId, custBotAccount.SubAccId)));
            await senderService.SendMessage(messengerBotId, chatId, message);
        }

        private async Task HandleMenuCommand(long messengerBotId, long chatId)
        {

            var bot = await botService.GetAsync(messengerBotId);
            var custBotAccount = await settingService.GetCustBotAccount(chatId, bot.BotId);
            var custBotMessenger = await settingService.GetCustBotMessenger(custBotAccount.CustId, custBotAccount.SubAccId);
            await senderService.SendMessage(messengerBotId, chatId, builderService.BuildMenuMessage(custBotMessenger.Language),replyMarkup:actionService.GetRootActions());
        }
    }
}