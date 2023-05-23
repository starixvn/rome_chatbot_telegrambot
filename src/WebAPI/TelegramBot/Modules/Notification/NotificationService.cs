using TelegramBot.Modules.Bot;
using TelegramBot.Modules.Function.Constants;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Notification
{
    public interface INotificationService
    {
        Task<Result> NotifyChangePassword(int custId, int subAccId);

        Task<Result> NotifyChangeSecurityCode(int custId, int subAccId);

        Task<Result> NotifyUnlink(int userId, long chatId, int botId, string language);

        Task<Result> NotifyWrongPassword(int custId, int subAccId, string ip);

        Task<Result> NotifySuccessCustomerCreated(int custId, int subAccId, string message);
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
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message, FunctionIds.ChangePasswordAlert);

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
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message, FunctionIds.ChangeSecurityCodeAlert);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyUnlink(int userId, long chatId, int botId, string language)
        {
            var bot = await botService.GetAsync(botId);
            await settingService.SendUnlinkMessage(userId, chatId, botId, bot.DisplayName, language);
            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifyWrongPassword(int custId, int subAccId, string ip)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);

            if (messenger == null || !messenger.IsFunctionAvailable(FunctionIds.WrongPasswordAlert))
            {
                return Result.CreateErrorResult();
            }

            var message = messageBuilder.BuildWrongPasswordAlertMessage(messenger.Language, ip);
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message, FunctionIds.WrongPasswordAlert);

            return Result.CreateSuccessfulResult();
        }

        public async Task<Result> NotifySuccessCustomerCreated(int custId, int subAccId, string message)
        {
            var messenger = await settingService.GetCustBotMessenger(custId, subAccId);
            await messageSender.SendMessage(messenger.UserId, new Messenger(messenger.BotId, messenger.ChatId), message);
            return Result.CreateSuccessfulResult();
        }
    }
}