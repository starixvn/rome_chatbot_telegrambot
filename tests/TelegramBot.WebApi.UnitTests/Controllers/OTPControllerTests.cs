using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.OTP;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.WebApi.Controllers;
using TelegramBot.WebApi.Params;
using Xunit;

namespace TelegramBot.WebApi.UnitTests.Controllers;

public class OTPControllerTests
{
    private readonly IOTPService otpService;
    private readonly ISettingService settingService;
    private readonly IMessageSender messageSender;
    private readonly IMessageBuilder messageBuilder;
    private readonly OTPController otpController;

    public OTPControllerTests()
    {
        otpService = Substitute.For<IOTPService>();
        settingService = Substitute.For<ISettingService>();
        messageSender = Substitute.For<IMessageSender>();
        messageBuilder = Substitute.For<IMessageBuilder>();
        otpController = new OTPController(otpService, settingService, messageSender, messageBuilder);
    }

    [Theory, AutoData]
    public async Task SendOTP_InvalidMessenger_ReturnBadRequest(SendOtpParam param)
    {
        // Arrange
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult<CustBotMessenger>(null));

        // Act
        var result = await otpController.SendOTP(param);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Theory, AutoData]
    public async Task SendOTP_ValidMessenger_ReturnOk(SendOtpParam param, CustBotMessenger messenger)
    {
        // Arrange
        messenger.Functions = new List<BotFunction> {
            new BotFunction(){FunctionId=FunctionIds.OTP, Enabled=true}
        };
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));

        // Act
        var result = await otpController.SendOTP(param);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Theory, AutoData]
    public async Task VerifyOTP_InvalidMessenger_ReturnBadRequest(VerifyOTPParam param)
    {
        // Arrange
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult<CustBotMessenger>(null));

        // Act
        await otpController.VerifyOTP(param);

        // Assert
        await otpService.DidNotReceive().VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), 5);
    }

    [Theory, AutoData]
    public async Task VerifyOTP_ValidMessenger_ReturnOk(VerifyOTPParam param, CustBotMessenger messenger)
    {
        // Arrange
        messenger.Functions = new List<BotFunction> {
            new BotFunction(){FunctionId=FunctionIds.OTP, Enabled=true}
        };
        settingService.GetCustBotMessenger(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(messenger));
        otpService.VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), 5).Returns(Task.FromResult(new Result(0)));

        // Act
        await otpController.VerifyOTP(param);

        // Assert
        await otpService.Received().VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), 5);
    }
}