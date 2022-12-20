using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.Models;
using TelegramBot.Infrastructure.Shared.Constants;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Message;

public class MessageBuilderTest
{
    private readonly MessageBuilder messageBuilder;
    private readonly IResourceService resourceService;

    public MessageBuilderTest()
    {
        resourceService = Substitute.For<IResourceService>();
        messageBuilder = new MessageBuilder(new TelegramFormatter(), resourceService);
    }

    [Fact]
    public void BuildAccountAlreadyLinkedMessage_Always_ReturnAccountAlreadyLinked()
    {
        const string expect =
            "Your Telegram has already linked with another account. Please unlink that account first then try again.";

        var result = messageBuilder.BuildAccountAlreadyLinkedMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildChangePasswordAlertMessage_Always_ReturnChangePasswordAlertMessage()
    {
        var expect = new List<string>
        {
            $"[Change Password Alert - {DateTime.Now:MM/dd/yyyy}]",
            "Your account password has been changed.",
            "If this wasn't you, please contact Customer Service Department for support",
        };

        var result = messageBuilder.BuildChangePasswordAlertMessage("EN");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildChangeSecurityCodeAlertMessage_Always_ReturnChangeSecurityCodeAlertMessage()
    {
        var expect = new List<string>
        {
            $"[Change Security Code Alert - {DateTime.Now:MM/dd/yyyy}]",
            "Your Security Code has been changed.",
            "If this wasn't you, please change password or contact Customer Service Department for support",
        };

        var result = messageBuilder.BuildChangeSecurityCodeAlertMessage("EN");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildOTPMessage_Always_ReturnOTPMessage()
    {
        const string expect =
            @"Please enter this OTP <code>192.168.9.9</code> to <b>the Agency Site</b> within 3 minutes to finish setting up your Telegram Chatbot.";

        var result = messageBuilder.BuildSettingOTPMessage("EN", "192.168.9.9");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildTelegramAccountAlreadyLinkedMessage_Always_ReturnTelegramAccountAlreadyLinkedMessage()
    {
        var expect =
            @"Your Telegram has already linked with this chatbot using another account. If this is not your account, please contact us for support.";

        var result = messageBuilder.BuildTelegramAccountAlreadyLinkedMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildUnderMaintenanceMessage_Always_ReturnUnderMaintenanceMessage()
    {
        var expect = new List<string>
        {
            "Sorry, the system is under maintenance from 20/3/2020 to 20/3/2030 GMT+8. Please come back later.",
            "很抱歉，服务器正在维护中，维护时间从20/3/2020至20/3/2030 (GMT+8)。请稍后访问。"
        };
        var result = messageBuilder.BuildUnderMaintenanceMessage("20/3/2020", "20/3/2030", "IBCBet");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildMenuMessage_Always_ReturnMenuMessage()
    {
        const string expect = @"Please select an action:";
        var result = messageBuilder.BuildMenuMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildWelcomeMessage_Always_ReturnWelcomeMessage()
    {
        var expect = new List<string>()
        {
            "Welcome jack.",
            "Your Telegram Chatbot setup is completed.",
            "You can view reports by interacting with this chatbot, and keep up with the latest information via automated notifications.",
            "You will receive your Yesterday Win Loss Report after 2:00 PM (GMT+8) everyday."
        };

        var result = messageBuilder.BuildWelcomeMessage("EN", "jack");

        Assert.Equal(string.Join(" ", expect), result);
    }

    [Fact]
    public void BuildNotRecognizedCommandMessage_Always_ReturnNotRecognizedCommandMessage()
    {
        const string expect = @"Sorry, we couldn't recognize your request. Which action would you like to take?";
        var result = messageBuilder.BuildNotRecognizedCommandMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildActionSelectedMessage_Always_ReturnActionSelectedMessage()
    {
        const string expect = @"Action Selected: actionName";
        var result = messageBuilder.BuildActionSelectedMessage("EN", "actionName");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildInvalidNumerOfDownlineRange_Always_ReturnInvalidNumerOfDownlineRange()
    {
        const string expect = @"""userResponse"" is invalid, please enter a valid number (max 10)";
        var result = messageBuilder.BuildInvalidNumberOfDownlineRange("EN", "userResponse", 10);

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildInvalidCustNameMessage_Always_ReturnInvalidCustNameMessage()
    {
        const string expect = @"""invalidCustName"" is invalid, please enter a valid username";
        var result = messageBuilder.BuildInvalidCustNameMessage("EN", "invalidCustName");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildNoDataMessage_Always_ReturnNoDataMessage()
    {
        const string expect = @"No information is available";
        var result = messageBuilder.BuildNoDataMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildAccountClosedMessage_Always_ReturnAccountClosedMessage()
    {
        const string expect = @"Your account is closed";
        var result = messageBuilder.BuildAccountClosedMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildAccountLockedMessage_Always_ReturnAccountLockedMessage()
    {
        const string expect = @"Your account is locked";
        var result = messageBuilder.BuildAccountLockedMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildUnlinkMessage_Always_ReturnUnlinkMessage()
    {
        const string expect = @"Your Telegram has been unlinked with Telegram.";
        var result = messageBuilder.BuildUnlinkMessage("EN", "Telegram");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildInvalidUsernameMessage_Always_ReturnInvalidUsernameMessage()
    {
        const string expect = @"Invalid username.";
        var result = messageBuilder.BuildInvalidUsernameMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildInvalidDownlinesMessage_Always_ReturnInvalidDownlinesMessage()
    {
        const string expect = @"Number of downlines must be from 1 to 10.";
        var result = messageBuilder.BuildInvalidDownlinesMessage("EN", 10);

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildUpdateNotificationSettingMessage_Enabled_ReturnUpdateNotificationSettingMessage()
    {
        const string expect = @"Notification turned on";
        var result = messageBuilder.BuildUpdateNotificationSettingMessage("EN", true);

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildUpdateNotificationSettingMessage_UnEnable_ReturnUpdateNotificationSettingMessage()
    {
        const string expect = @"Notification turned off";
        var result = messageBuilder.BuildUpdateNotificationSettingMessage("EN", false);

        Assert.Equal(expect, result);
    }

    [Fact]
    public async Task BuildActionName_Always_ReturnActionName()
    {
        var botResource = new BotResource("a", "b", "en", "BotName");
        resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<BotResource>(botResource));

        var result = await messageBuilder.BuildActionName("EN", 13);

        Assert.Equal(botResource.Value, result);
    }

    [Fact]
    public void BuildMessage_Always_ReturnReplyMarkupMessage()
    {
        var result = messageBuilder.BuildMessage("EN", new List<MessageButton> { new MessageButton("setting", "/setting") }, "EN");

        Assert.IsType<Infrastructure.Message.Models.Message>(result);
        Assert.Single(result.ReplyMarkup.InlineKeyboard);
    }

    [Fact]
    public void BuildMessageWithBackButton_Always_ReturnReplyMarkupMessage()
    {
        var result = messageBuilder.BuildMessageWithBackButton("EN", new List<MessageButton> { new("setting", "/setting") }, 0, "EN", false);

        Assert.IsType<Infrastructure.Message.Models.Message>(result);
        Assert.Equal(2, result.ReplyMarkup.InlineKeyboard.Count());
    }

    [Fact]
    public void BuildStopUsingChatbotConfirmMessage_Always_ReturnMainMenuButton()
    {
        var textBtn =
            "You're about to <b>Stop Using Chatbot</b>. Your Telegram will be unlinked with Telegram and you cannot use Telegram anymore. Would you like to continue?";

        var result = messageBuilder.BuildStopUsingChatbotConfirmMessage("EN", "Telegram");

        Assert.IsType<Infrastructure.Message.Models.Message>(result);
        Assert.Single(result.ReplyMarkup.InlineKeyboard);
        Assert.Equal(textBtn, result.Text);
    }

    [Fact]
    public void BuildIntroductionMessage_SystemIs789_ReturnsCorrectMessage()
    {
        // Arrange
        var expectedResult = string.Join("\n",
            "Please log in to <b>the Agency Site</b>, access Chatbot function and follow the instructions to setup Telegram Chatbot.",
            "Vui lòng đăng nhập vào <b>trang Agency</b>, đi đến chức năng Chatbot và làm theo hướng dẫn để cài đặt Telegram Chatbot.");

        // Act
        var result = messageBuilder.BuildIntroductionMessage("789Y");

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void BuildIntroductionMessage_SystemIsNOT789_ReturnsCorrectMessage()
    {
        // Arrange
        var expectedResult = string.Join("\n",
            "Please log in to <b>the Agency Site</b>, access Chatbot function and follow the instructions to setup Telegram Chatbot.",
            "请登入<b>代理网</b>，访问Chatbot功能并按照指示设置Telegram Chatbot");

        // Act
        var result = messageBuilder.BuildIntroductionMessage("IBCBET");

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void BuildMainMenuButtonText_Always_ReturnsCorrectValue()
    {
        // Act
        var result = messageBuilder.BuildMainMenuButtonText(LanguageCodes.English);

        // Assert
        Assert.Equal("\u2630 Main Menu", result);
    }

    [Fact]
    public void BuildLanguageUpdatedMessage_Always_ReturnsCorrectValue()
    {
        // Act
        var result = messageBuilder.BuildLanguageUpdatedMessage(LanguageCodes.English);

        // Assert
        Assert.Equal("Language updated.", result);
    }

    [Fact]
    public void BuildMinimizeUMMessage_Always_ReturnsCorrectValue()
    {
        // Assert
        var starTime = "2022/07/25 03:00";
        var endTime = "2022/07/25 04:00";
        var expectedResult = "We're updating this function for a better experience. Thus, this function is turned off temporarily (2022/07/25 03:00 - 2022/07/25 04:00 (GMT+8)). Please try again later and sorry for the inconvenience.";

        // Act
        var result = messageBuilder.BuildMinimizeUMMessage(LanguageCodes.English, starTime, endTime);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void BuildGeneralCommandsHelpMessage_Always_ReturnsCorrectValue()
    {
        // Arrange
        var botName = "IBCBET";
        var expectedResult = new StringBuilder();
        expectedResult.Append("You can run IBCBET using the following commands:\n\n");
        expectedResult.Append("/menu - Open Main Menu of IBCBET\n");
        expectedResult.Append("/help - List all commands\n");
        expectedResult.Append("/settings, /setting - Open Settings Menu\n");
        expectedResult.Append("/stop, /unlink - Unlink IBCBET");

        // Act
        var result = messageBuilder.BuildGeneralCommandsHelpMessage(LanguageCodes.English, botName);

        // Assert
        Assert.Equal(expectedResult.ToString(), result);
    }

    [Fact]
    public void BuildWrongPasswordAlertMessage_Always_ReturnsCorrectValue()
    {
        // Arrange
        var ip = "127.0.0.1";
        var expectedResult = new StringBuilder();
        expectedResult.Append($"[Wrong Password Alert - {DateTime.Now:MM/dd/yyyy}]\n");
        expectedResult.Append("Your account password has been entered incorrectly several times.\n");
        expectedResult.Append($"Requested IP: {ip}\n");
        expectedResult.Append("If this wasn't you, please contact Customer Service Department for support");

        // Act
        var result = messageBuilder.BuildWrongPasswordAlertMessage(LanguageCodes.English, ip);

        // Assert
        Assert.Equal(expectedResult.ToString(), result);
    }
}