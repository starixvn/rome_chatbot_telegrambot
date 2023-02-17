using System.Threading.Tasks;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Modules.Notification;
using TelegramBot.WebApi.Controllers;
using TelegramBot.WebApi.Params;
using Xunit;

namespace TelegramBot.WebApi.UnitTests;

[Trait("Controllers", "Notification")]
public class NotificationControllerTests
{
    private readonly INotificationService notificationService;
    private readonly NotificationController notificationController;

    public NotificationControllerTests()
    {
        notificationService = Substitute.For<INotificationService>();
        notificationController = new NotificationController(notificationService);
    }

    [Theory, AutoData]
    public async Task NotifyChangePassword_Always_CallNotificationServiceNotifyChangePassword(AlertParam param)
    {
        // Act
        await notificationController.NotifyChangePassword(param);

        // Assert
        await notificationService.Received().NotifyChangePassword(param.CustId, param.SubAccId);
    }

    [Theory, AutoData]
    public async Task NotifyChangeSecurityCode_Always_CallNotificationServiceNotifyChangeSecurityCode(AlertParam param)
    {
        // Act
        await notificationController.NotifyChangeSecurityCode(param);

        // Assert
        await notificationService.Received().NotifyChangeSecurityCode(param.CustId, param.SubAccId);
    }

    [Theory, AutoData]
    public async Task Unlink_Always_CallNotificationServiceNotifyChangeSecurityCode(int userId, UnlinkNotificationParam param)
    {
        // Act
        await notificationController.Unlink(userId, param);

        // Assert
        await notificationService.Received().NotifyUnlink(userId, param.ChatId, param.BotId, param.Language);
    }

    [Theory, AutoData]
    public async Task NotyfyWrongPassword_Always_CallNotificationServiceNotifyWrongPassword(AlertParam param)
    {
        // Act
        await notificationController.NotifyWrongPassword(param);

        // Assert
        await notificationService.Received().NotifyWrongPassword(param.CustId, param.SubAccId, param.IP);
    }
}