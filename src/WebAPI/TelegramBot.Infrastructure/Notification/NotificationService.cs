using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Notification
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId, string ip);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip);

        Task<Result> Notify(int botId, long chatid, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly IBotService botService;
        private readonly ISettingService settingService;
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageSender messageSender;

        public NotificationService(
            IBotService botService,
            ISettingService settingService,
            IMessageBuilder messageBuilder,
            IMessageSender messageSender)
        {
            this.botService = botService;
            this.settingService = settingService;
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
        }

        public async Task<Result> Notify(int botId, long chatId, string message)
        {
            await messageSender.SendMessage(new Messenger(botId, chatId), message);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyChangePassword(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null || !messenger.IsFunctionAvailable(FunctionIds.ChangePasswordAlert))
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildChangePasswordAlertMessage(messenger.Language, ip);
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null || !messenger.IsFunctionAvailable(FunctionIds.ChangeSecurityCodeAlert))
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildChangeSecurityCodeAlertMessage(messenger.Language, ip);
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);

            return Result.CreateSuccessfulResult();
        }
    }
}