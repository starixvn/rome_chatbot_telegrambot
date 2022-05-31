using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Infrastructure.Action.Validators;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action.Validators;

public class NumberOfDownlinesValidatorTest
{
    private readonly int maxValue;
    private readonly IMessageBuilder messageBuilder;
    private readonly NumberOfDownlinesValidator numberOfDownlinesValidator;
    public NumberOfDownlinesValidatorTest()
    {
        messageBuilder = Substitute.For<IMessageBuilder>();
        maxValue = 100;
        numberOfDownlinesValidator = new NumberOfDownlinesValidator(messageBuilder, maxValue);
    }

    [Fact]
    public async Task Validate_ValidNumber_ReturnSuccessful()
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        var expectResult = Result.CreateSuccessfulResult();
        
        var result = numberOfDownlinesValidator.Validate(account, "3");
        
        Assert.Equal(expectResult.Code,result.Result.Code);
        Assert.Equal(expectResult.Message,result.Result.Message);
    } 
    public static IEnumerable<object[]> RequestsNumber()
    {
        yield return new object[] { "-1"};
        yield return new object[] { "102"};
        yield return new object[] { ""};
    }
    [Theory]
    [MemberData(nameof(RequestsNumber))]
    public async Task Validate_inValidNumber_ReturnErrorResult(string userResponse)
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        var expectResult = Result.CreateErrorResult();
        
        var result = numberOfDownlinesValidator.Validate(account, userResponse);
        
        Assert.Equal(expectResult.Code,result.Result.Code);
        Assert.Equal(expectResult.Message,result.Result.Message);
    }
}