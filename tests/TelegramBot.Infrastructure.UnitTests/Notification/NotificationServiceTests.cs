using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Notification;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;
using NSubstitute;
using TelegramBot.Infrastructure.Setting.Models;
using System.Text.Json;
using AutoFixture.Xunit2;

namespace TelegramBot.Infrastructure.UnitTests.Notification;

public class NotificationServiceTests
{
    private readonly ISettingService settingService;
    private readonly IMessageBuilder messageBuilder;
    private readonly IMessageSender messageSender;
    private readonly IBotService botService;
    private readonly INotificationService notificationService;

    public NotificationServiceTests()
    {
        settingService = Substitute.For<ISettingService>();
        messageBuilder = Substitute.For<IMessageBuilder>();
        messageSender = Substitute.For<IMessageSender>();
        botService = Substitute.For<IBotService>();
        notificationService = new NotificationService(botService, settingService, messageBuilder, messageSender);
    }

    [Fact]
    public async Task NotifyChangePassword_MessengerIsNull_ReturnErrorResult()
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        CustBotMessenger messenger = null;
        var expectResult = Result.CreateErrorResult();
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));

        // Act
        var result = await notificationService.NotifyChangePassword(custId, subAccId);

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }

    [Theory, AutoData]
    public async Task NotifyChangePassword_FunctionNotAvailable_ReturnErrorResult(CustBotMessenger messenger)
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        var expectResult = Result.CreateErrorResult();
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));

        // Act
        var result = await notificationService.NotifyChangePassword(custId, subAccId);

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }
}
