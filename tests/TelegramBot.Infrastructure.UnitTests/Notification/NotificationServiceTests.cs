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
using System.Collections.Generic;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Bot.Models;

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
    public async Task NotifyChangePassword_ValidMessenger_ReturnSuccessfulResult(CustBotMessenger messenger)
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        var expectResult = Result.CreateSuccessfulResult();
        messenger.Functions = new List<BotFunction> { new BotFunction() { FunctionId = FunctionIds.ChangePasswordAlert } };
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));
        messageBuilder.BuildChangePasswordAlertMessage(messenger.Language).Returns("ChangePasswordAlertMessage");

        // Act
        var result = await notificationService.NotifyChangePassword(custId, subAccId);

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }

    [Fact]
    public async Task NotifyChangeSecurityCode_MessengerIsNull_ReturnErrorResult()
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        CustBotMessenger messenger = null;
        var expectResult = Result.CreateErrorResult();
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));

        // Act
        var result = await notificationService.NotifyChangeSecurityCode(custId, subAccId);

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }

    [Theory, AutoData]
    public async Task NotifyChangeSecurityCode_ValidMessenger_ReturnSuccessfulResult(CustBotMessenger messenger)
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        var expectResult = Result.CreateSuccessfulResult();
        messenger.Functions = new List<BotFunction> { new BotFunction() { FunctionId = FunctionIds.ChangeSecurityCodeAlert } };
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));
        messageBuilder.BuildChangeSecurityCodeAlertMessage(messenger.Language).Returns("ChangeSecurityCodeAlertMessage");

        // Act
        var result = await notificationService.NotifyChangeSecurityCode(custId, subAccId);

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }

    [Theory, AutoData]
    public async Task NotifyUnlink_Always_SendUnlinkMessage(int userId, long chatId, int botId, string language, BotEntity bot)
    {
        // Arrange
        botService.GetAsync(botId).Returns(Task.FromResult(bot));

        // Act
        var result = await notificationService.NotifyUnlink(userId, chatId, botId, language);

        // Assert
        await settingService.Received().SendUnlinkMessage(userId, chatId, botId, bot.DisplayName, language);
        Assert.Equal(JsonSerializer.Serialize(Result.CreateSuccessfulResult()), JsonSerializer.Serialize(result));
    }

    [Fact]
    public async Task NotifyWrongPassword_MessengerIsNull_ReturnErrorResult()
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        CustBotMessenger messenger = null;
        var expectResult = Result.CreateErrorResult();
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));

        // Act
        var result = await notificationService.NotifyWrongPassword(custId, subAccId, ":1");

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }

    [Theory, AutoData]
    public async Task NotifyWrongPassword_ValidMessenger_ReturnSuccessfulResult(CustBotMessenger messenger, string ip)
    {
        // Arrange
        int custId = 1;
        int subAccId = 2;
        var expectResult = Result.CreateSuccessfulResult();
        messenger.Functions = new List<BotFunction> { new BotFunction() { FunctionId = FunctionIds.WrongPasswordAlert } };
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));
        messageBuilder.BuildChangeSecurityCodeAlertMessage(messenger.Language).Returns("WrongPasswordAlertMessage");

        // Act
        var result = await notificationService.NotifyWrongPassword(custId, subAccId, ip);

        // Assert
        Assert.Equal(JsonSerializer.Serialize(expectResult), JsonSerializer.Serialize(result));
    }
}
