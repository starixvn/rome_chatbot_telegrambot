using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Notification
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId, string ip);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip);
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

        public async Task<Result> NotifyChangePassword(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null)
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildChangePasswordAlertMessage(messenger.Language, ip);
            await messageSender.SendMessage(messenger, message);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null)
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildChangeSecurityCodeAlertMessage(messenger.Language, ip);
            await messageSender.SendMessage(messenger, message);

            return Result.CreateSuccessfulResult();
        }
    }
}