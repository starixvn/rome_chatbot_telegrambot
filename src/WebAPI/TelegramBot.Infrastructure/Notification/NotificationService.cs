using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Notification
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId);

        Task<Result> NotifyUnlink(int userId, long chatId, int botId, string language);
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

        public async Task<Result> NotifyChangePassword(int custId, int subAccId)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null || !messenger.IsFunctionAvailable(FunctionIds.ChangePasswordAlert))
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildChangePasswordAlertMessage(messenger.Language);
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyChangeSecurityCode(int custId, int subAccId)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null || !messenger.IsFunctionAvailable(FunctionIds.ChangeSecurityCodeAlert))
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildChangeSecurityCodeAlertMessage(messenger.Language);
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyUnlink(int userId, long chatId, int botId, string language)
        {
            var bot = await botService.GetAsync(botId);
            await settingService.SendUnlinkMessage(userId, chatId, botId, bot.DisplayName, language);
            return Result.CreateSuccessfulResult();
        }
    }
}