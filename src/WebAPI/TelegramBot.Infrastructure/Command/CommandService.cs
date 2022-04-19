using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;

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

        public CommandService(IBuilderService builderService, ISenderService senderService, ISettingService settingService, IBotService botService)
        {
            this.builderService = builderService;
            this.senderService = senderService;
            this.settingService = settingService;
            this.botService = botService;
        }

        public async Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback)
        {
            if (messengerBotId == 0 || chatId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (message.Trim().Equals("/help"))
            {
                return;
            }

            var custBotMessenger = await settingService.GetCustBotMessenger(0, 0);
            await senderService.SendMessage(messengerBotId, chatId, message);
        }

        private async Task HandleHelpCommand(long messengerBotId, long chatId)
        {
        }
    }
}