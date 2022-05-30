using System.Collections.Generic;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Shared.Caching;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Message;

public class MessageBuilderTest
{
    private MessageBuilder messageBuilder;

    public MessageBuilderTest()
    {
        messageBuilder = new MessageBuilder(new TelegramFormatter(),
            new ResourceService(Substitute.For<ICacheServiceFactory>(),
                Substitute.For<IDynamicRepository>()));
    }

    [Fact]
    public void BuildAccountAlreadyLinkedMessage_Always_ReturnAccountAlreadyLinked()
    {
        const string expect = "Your Telegram's already linked with another Account.Please unlink that Account first then try again.";
        
        var result = messageBuilder.BuildAccountAlreadyLinkedMessage("EN");
        
        Assert.Equal(expect,result);
    }
    
    [Fact]
    public void BuildChangePasswordAlertMessage_Always_ReturnChangePasswordAlertMessage()
    {
        
        var expect = new List<string>()
        {
            "[Change Password Alert - 20220530]",
            "Your Alpha Site account's Password has been changed.",
            "<b>Requested IP</b>: 192.168.9.9",
            "If this wasn't you, please contact us for support",
        };
        
        var result = messageBuilder.BuildChangePasswordAlertMessage("EN","192.168.9.9");
        
        Assert.Equal(string.Join("\n", expect),result);
    }
}