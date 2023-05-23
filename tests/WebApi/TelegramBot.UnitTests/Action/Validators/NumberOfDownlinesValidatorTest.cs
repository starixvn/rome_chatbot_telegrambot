using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Modules.Action.Validators;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Models;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Action.Validators;

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
    public void Validate_ValidNumber_ReturnSuccessful()
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        var expectResult = Result.CreateSuccessfulResult();

        var result = numberOfDownlinesValidator.Validate(account, "3");

        Assert.Equal(expectResult.Code, result.Result.Code);
        Assert.Equal(expectResult.Message, result.Result.Message);
    }

    public static IEnumerable<object[]> RequestsNumber()
    {
        yield return new object[] { "-1" };
        yield return new object[] { "102" };
        yield return new object[] { "" };
    }

    [Theory]
    [MemberData(nameof(RequestsNumber))]
    public void Validate_inValidNumber_ReturnErrorResult(string userResponse)
    {
        var account = new CustBotAccount() { CustId = 1, CustomerLevel = 3 };
        var expectResult = Result.CreateErrorResult();

        var result = numberOfDownlinesValidator.Validate(account, userResponse);

        Assert.Equal(expectResult.Code, result.Result.Code);
        Assert.Equal(expectResult.Message, result.Result.Message);
    }
}