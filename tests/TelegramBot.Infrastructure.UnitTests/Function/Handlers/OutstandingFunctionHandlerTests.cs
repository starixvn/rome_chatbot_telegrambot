using System.Threading.Tasks;
using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Models.Outstanding;
using Alpha.API.Client.Services;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Function.Handlers;

public class OutstandingFunctionHandlerTests
{
    private readonly IMessageBuilder messageBuilder;
    private readonly IMessageFormatter messageFormatter;
    private readonly IOutstandingService outstandingService;
    private readonly ICustomerService customerService;
    private readonly WebAPIConfiguration webAPIConfiguration;
    private readonly OutstandingFunctionHandler outstandingFunctionHandler;

    public OutstandingFunctionHandlerTests()
    {
        messageBuilder = Substitute.For<IMessageBuilder>();
        messageFormatter = Substitute.For<IMessageFormatter>();
        outstandingService = Substitute.For<IOutstandingService>();
        customerService = Substitute.For<ICustomerService>();
        webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
        outstandingFunctionHandler = new OutstandingFunctionHandler(messageBuilder, messageFormatter, outstandingService, customerService, webAPIConfiguration);
    }

    [Theory, AutoData]
    public async Task Handle_InvalidParameter_ReturnsErrorResult(CustBotAccount account)
    {
        // Arrange
        var parameters = new[] { "" };
        var expectedResult = Result.CreateErrorResult();

        // Act
        var actualResult = await outstandingFunctionHandler.Handle(account, parameters, null);

        // Assert
        Assert.True(actualResult.Code == expectedResult.Code && actualResult.Message == expectedResult.Message);
    }

    [Fact]
    public async Task Handle_InvalidDirectDownline_ReturnsErrorResult()
    {
        // Arrange
        var parameters = new[] { "test01" };
        var expectedResult = Result.CreateErrorResult("InvalidUsernameMessage");
        CustBotAccount account = new() { CustId = 1, CustomerLevel = 3 };
        messageBuilder.BuildInvalidUsernameMessage(Arg.Any<string>()).Returns("InvalidUsernameMessage");
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(new DownlineInfo(2, 1, false)));

        // Act
        var actualResult = await outstandingFunctionHandler.Handle(account, parameters, null);

        // Assert
        Assert.True(actualResult.Code == expectedResult.Code && actualResult.Message == expectedResult.Message);
    }

    [Theory, AutoData]
    public async Task Handle_ValidDirectDownline_CallsGetTopOutstanding(OutstandingReport outstanding)
    {
        // Arrange
        var parameters = new[] { "test01" };
        CustBotAccount account = new() { CustId = 1, CustomerLevel = 3, Language = "en-US" };
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(new DownlineInfo(2, 2, false)));
        outstandingService.GetDownlineOutstanding(Arg.Any<int>(), Arg.Any<byte>(), Arg.Any<int>()).Returns(Task.FromResult(outstanding));

        // Act
        await outstandingFunctionHandler.Handle(account, parameters, null);

        // Assert
        await outstandingService.Received().GetDownlineOutstanding(1, 3, 2);
    }

    [Theory, AutoData]
    public async Task Handle_ValidTopQueryNumber_CallsGetTopOutstanding(OutstandingReport outstanding)
    {
        // Arrange
        var parameters = new[] { "10" };
        webAPIConfiguration.OutstandingMaxTopQuery = 20;
        CustBotAccount account = new() { CustId = 1, CustomerLevel = 3, Language = "en-US" };
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult<DownlineInfo>(null));
        outstandingService.GetTopOutstanding(Arg.Any<int>(), Arg.Any<byte>(), Arg.Any<int>()).Returns(Task.FromResult(outstanding));

        // Act
        await outstandingFunctionHandler.Handle(account, parameters, null);

        // Assert
        await outstandingService.Received().GetTopOutstanding(1, 3, 10);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("21")]
    public async Task Handle_InvalidTopQueryNumber_CallsGetTopOutstanding(string topQuery)
    {
        // Arrange
        var parameters = new[] { topQuery };
        webAPIConfiguration.OutstandingMaxTopQuery = 20;
        var expectedResult = Result.CreateErrorResult("InvalidDownlinesMessage");
        messageBuilder.BuildInvalidDownlinesMessage(Arg.Any<string>(), Arg.Any<int>()).Returns("InvalidDownlinesMessage");
        CustBotAccount account = new() { CustId = 1, CustomerLevel = 3, Language = "en-US" };
        customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult<DownlineInfo>(null));

        // Act
        var actualResult = await outstandingFunctionHandler.Handle(account, parameters, null);

        // Assert
        Assert.True(actualResult.Code == expectedResult.Code && actualResult.Message == expectedResult.Message);
    }
}
