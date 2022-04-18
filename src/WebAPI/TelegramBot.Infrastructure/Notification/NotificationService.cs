using System.Threading.Tasks;
using TelegramBot.Common.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;

namespace TelegramBot.Infrastructure.Notification
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId, string ip);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip);
    }

    public class NotificationService : INotificationService
    {
        private readonly ISettingService settingService;
        private readonly IBuilderService builderService;
        private readonly ISenderService senderService;

        public NotificationService(
            ISettingService settingService,
            IBuilderService builderService,
            ISenderService senderService)
        {
            this.settingService = settingService;
            this.builderService = builderService;
            this.senderService = senderService;
        }

        public async Task<Result> NotifyChangePassword(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessengerAsync(custId, subAccId);

            if (messenger == null)
            {
                return Result.CreateErrorResult();
            }

            var message = builderService.BuildChangePasswordAlertMessage(messenger.Language, ip);
            await senderService.SendMessage(messenger.BotId, messenger.ChatId, message);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyChangeSecurityCode(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessengerAsync(custId, subAccId);

            if (messenger == null)
            {
                return Result.CreateErrorResult();
            }

            var message = builderService.BuildChangeSecurityCodeAlertMessage(messenger.Language, ip);
            await senderService.SendMessage(messenger.BotId, messenger.ChatId, message);

            return Result.CreateSuccessfulResult();
        }
    }
}