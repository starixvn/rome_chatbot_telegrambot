using Newtonsoft.Json;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Notification;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;
using NSubstitute;

namespace TelegramBot.Infrastructure.UnitTests.Notification
{
    public class NotificationServiceTests
    {
        private readonly IBotService botService;
        private readonly ISettingService settingService;
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageSender messageSender;
        private readonly INotificationService notificationService;

        public NotificationServiceTests()
        {
            botService = Substitute.For<IBotService>();
            settingService = Substitute.For<ISettingService>();
            messageBuilder = Substitute.For<IMessageBuilder>();
            messageSender = Substitute.For<IMessageSender>();
            notificationService = new NotificationService(botService, settingService, messageBuilder, messageSender);
        }

        [Fact]
        public async Task Notify_Always_ReturnSuccessMessage()
        {
            // Arrange
            int botId = 1;
            long chatId = 123;
            string message = "start";

            // Act
            var result = await notificationService.Notify(botId, chatId, message);

            // Assert
            var messenger = new Messenger(botId, chatId);
            await messageSender.ReceivedWithAnyArgs().SendMessage(messenger, message);
            Assert.Equal(JsonConvert.SerializeObject(new Result(0)), JsonConvert.SerializeObject(result));
        }
    }
}