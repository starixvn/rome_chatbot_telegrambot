using System.Threading.Tasks;
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
        private readonly ISenderService senderService;
        private readonly ISettingService settingService;

        public CommandService(ISenderService senderService, ISettingService settingService)
        {
            this.senderService = senderService;
            this.settingService = settingService;
        }

        public async Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback)
        {
            if (messengerBotId == 0 || chatId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var custBotMessenger = await settingService.GetCustBotMessengerAsync(0, 0);
            await senderService.SendMessage(messengerBotId, chatId, message).ConfigureAwait(false);
        }
    }
}
