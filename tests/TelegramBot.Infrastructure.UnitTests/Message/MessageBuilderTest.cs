using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.Models;
using TelegramBot.Infrastructure.Shared.Caching;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Message;

public class MessageBuilderTest
{
    private readonly MessageBuilder messageBuilder;
    private readonly IResourceService resourceService;
    public MessageBuilderTest()
    {
        resourceService = Substitute.For<IResourceService>();
        messageBuilder = new MessageBuilder(new TelegramFormatter(),resourceService);
    }

    [Fact]
    public void BuildAccountAlreadyLinkedMessage_Always_ReturnAccountAlreadyLinked()
    {
        const string expect =
            "Your Telegram's already linked with another Account.Please unlink that Account first then try again.";

        var result = messageBuilder.BuildAccountAlreadyLinkedMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildChangePasswordAlertMessage_Always_ReturnChangePasswordAlertMessage()
    {
        var expect = new List<string>()
        {
            $"[Change Password Alert - {DateTime.Now:yyyyMMdd}]",
            "Your Alpha Site account's Password has been changed.",
            "<b>Requested IP</b>: 192.168.9.9",
            "If this wasn't you, please contact us for support",
        };

        var result = messageBuilder.BuildChangePasswordAlertMessage("EN", "192.168.9.9");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildChangeSecurityCodeAlertMessage_Always_ReturnChangeSecurityCodeAlertMessage()
    {
        var expect = new List<string>()
        {
            $"[Change Security Code Alert - {DateTime.Now:yyyyMMdd}]",
            "Your Security Code has been changed.",
            "<b>Requested IP</b>: 192.168.9.9",
            "If this wasn't you, please change password or contact us for support",
        };

        var result = messageBuilder.BuildChangeSecurityCodeAlertMessage("EN", "192.168.9.9");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildOTPMessage_Always_ReturnOTPMessage()
    {
        const string expect =
            @"Please enter this private OTP to <b>the agent site</b> within 3 minutes to finish setting up your Telegram Chatbot. <b>OTP:</b> <code>192.168.9.9</code>";

        var result = messageBuilder.BuildOTPMessage("EN", "192.168.9.9");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildTelegramAccountAlreadyLinkedMessage_Always_ReturnTelegramAccountAlreadyLinkedMessage()
    {
        var expect =
            @"Your Telegram's already linked to this chatbot using other Account. If this isn't your Account, please contact us for support.";

        var result = messageBuilder.BuildTelegramAccountAlreadyLinkedMessage("EN");

        Assert.Equal(expect, result);
    }

    [Fact]
    public void BuildUnderMaintenanceMessage_Always_ReturnUnderMaintenanceMessage()
    {
        var expect = new List<string>()
        {
            "Sorry, the system is under maintenance from 20/3/2020 to 20/3/2030 GMT+8. Please come back later.",
            "Sorry, the system is under maintenance from 20/3/2020 to 20/3/2030 GMT+8. Please come back later.",
            "Sorry, the system is under maintenance from 20/3/2020 to 20/3/2030 GMT+8. Please come back later."
        };
        var result = messageBuilder.BuildUnderMaintenanceMessage("20/3/2020", "20/3/2030");

        Assert.Equal(string.Join("\n", expect), result);
    }

    [Fact]
    public void BuildGeneralCommandsHelpMessage_Always_ReturnGeneralCommandsHelpMessage()
    {
        var expect = new List<string>()
        {
            "You can operate Telegram using following commands:\n",
            "/menu - Open Main Menu of Telegram",
            "/help - List all commands",
            "/settings, /setting - Open Settings Menu",
            "/stop, /unlink - Unlink Telegram",
        };
        var result = messageBuilder.BuildGeneralCommandsHelpMessage("EN", "Telegram");

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
        const string expect = @"Welcome jack. Your Telegram Chatbot setup is completed. This bot can assist you to view report more easily or keep you posted with the latest important notifications. You will receive your yesterday's win loss after 2:00 PM (GMT+8) daily.";
        var result = messageBuilder.BuildWelcomeMessage("EN","jack");

        Assert.Equal(expect, result);
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
        var result = messageBuilder.BuildActionSelectedMessage("EN","actionName");

        Assert.Equal(expect, result);
    }
    
    [Fact]
    public void BuildInvalidNumerOfDownlineRange_Always_ReturnInvalidNumerOfDownlineRange()
    {
        const string expect = @"""userResponse"" is invalid, please enter valid number (max 10)";
        var result = messageBuilder.BuildInvalidNumerOfDownlineRange("EN","userResponse",10);

        Assert.Equal(expect, result);
    }  
    
    [Fact]
    public void BuildInvalidCustNameMessage_Always_ReturnInvalidCustNameMessage()
    {
        const string expect = @"""invalidCustName"" is invalid, please enter valid username";
        var result = messageBuilder.BuildInvalidCustNameMessage("EN","invalidCustName");

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
        const string expect = @"Your Account is closed";
        var result = messageBuilder.BuildAccountClosedMessage("EN");

        Assert.Equal(expect, result);
    }
    
    [Fact]
    public void BuildAccountLockedMessage_Always_ReturnAccountLockedMessage()
    {
        const string expect = @"Your Account is locked";
        var result = messageBuilder.BuildAccountLockedMessage("EN");

        Assert.Equal(expect, result);
    }    
    
    [Fact]
    public void BuildUnlinkMessage_Always_ReturnUnlinkMessage()
    {
        const string expect = @"Your telegram has been unlinked with Telegram successfully.";
        var result = messageBuilder.BuildUnlinkMessage("EN","Telegram");

        Assert.Equal(expect, result);
    }
    
    [Fact]
    public void BuildInvalidUsernameMessage_Always_ReturnInvalidUsernameMessage()
    {
        const string expect = @"Invalid username";
        var result = messageBuilder.BuildInvalidUsernameMessage("EN");

        Assert.Equal(expect, result);
    } 
    
    [Fact]
    public void BuildInvalidDownlinesMessage_Always_ReturnInvalidDownlinesMessage()
    {
        const string expect = @"Number of downlines must be from 1 to 10.";
        var result = messageBuilder.BuildInvalidDownlinesMessage("EN",10);

        Assert.Equal(expect, result);
    }   
    
    [Fact]
    public void BuildUpdateNotificationSettingMessage_Always_ReturnUpdateNotificationSettingMessage()
    {
        const string expect = @"Turn on notification";
        var result = messageBuilder.BuildUpdateNotificationSettingMessage("EN",true);

        Assert.Equal(expect, result);
    }  
    
    [Fact]
    public async Task BuildActionName_Always_ReturnActionName()
    {
        const string expect = @"Turn on notification";
        var botResource = new BotResource("a", "b", "en","BotName");
        resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<BotResource>(botResource));
        
        var result =await messageBuilder.BuildActionName("EN",13);

        Assert.Equal(botResource.Value, result);
    }
}