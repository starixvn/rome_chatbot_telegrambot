using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Alpha.API.Client.Services;
using Alpha.API.Client.Models.WinLoss;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Shared.Models;
using Newtonsoft.Json;
using Xunit;
using NSubstitute;

namespace TelegramBot.Infrastructure.UnitTests.Function.Handlers
{
    public class YesterdayWinLossFunctionHandlerTests
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly TelegramFormatter telegramFormatter;
        private readonly ICustomerService customerService;
        private readonly IWinLossService winLossService;
        private readonly YesterdayWinLossFunctionHandler yesterdayWinLossFunctionHandler;
        private static DateTime yesterday = DateTime.Today.AddDays(-1);

        public YesterdayWinLossFunctionHandlerTests()
        {
            messageBuilder = Substitute.For<IMessageBuilder>();
            telegramFormatter = Substitute.For<TelegramFormatter>();
            customerService = Substitute.For<ICustomerService>();
            winLossService = Substitute.For<IWinLossService>();

            yesterdayWinLossFunctionHandler = new YesterdayWinLossFunctionHandler(
                messageBuilder,
                telegramFormatter,
                winLossService,
                customerService
            );
        }

        [Fact]
        public async Task Handle_NullParameters_ReturnErrorResult()
        {
            // Arrange
            var account = new CustBotAccount();
            string[] parameters = { "" };

            // Act
            var result = await yesterdayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new Result(1)), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> MyYesterdayWinLossDataTests()
        {
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Super },
                new[] {"m"},
                new WinLossMaster(10, 1, 1, 1, 1, 1, 1),
                new[] { $"[Yesterday Win Loss - {yesterday:yyyyMMdd}]", "<b>TURNOVER</b>:\n    10.00", "<b>GROSS COMM</b>:\n    1.00", "<b>MASTER TOTAL</b>:\n    1.00", "<b>SUPER WIN LOSS</b>:\n    1.00","<b>SUPER COMM</b>:\n    1.00", "<b>SUPER TOTAL</b>:\n    1.00", "<b>COMPANY</b>:\n    1.00" }
            };
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Master },
                new[] {"m"},
                new WinLossAgent(10, 1, 1, 1, 1, 1, 1),
                new[] { $"[Yesterday Win Loss - {yesterday:yyyyMMdd}]", "<b>TURNOVER</b>:\n    10.00", "<b>GROSS COMM</b>:\n    1.00", "<b>AGENT TOTAL</b>:\n    1.00", "<b>MASTER WIN LOSS</b>:\n    1.00", "<b>MASTER COMM</b>:\n    1.00", "<b>MASTER TOTAL</b>:\n    1.00", "<b>COMPANY</b>:\n    1.00" }
            };
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Agent },
                new[] {"m"},
                new WinLossMember(10, 1, 1, 1, 1, 1, 1, 1, 1),
                new[] { $"[Yesterday Win Loss - {yesterday:yyyyMMdd}]", "<b>TURNOVER</b>:\n    10.00", "<b>GROSS COMM</b>:\n    1.00", "<b>MEMBER WIN LOSS</b>:\n    1.00", "<b>MEMBER COMM</b>:\n    1.00", "<b>MEMBER TOTAL</b>:\n    1.00", "<b>AGENT WIN LOSS</b>:\n    1.00", "<b>AGENT COMM</b>:\n    1.00","<b>AGENT TOTAL</b>:\n    1.00", "<b>COMPANY</b>:\n    1.00" }
            };
        }

        [Theory]
        [MemberData(nameof(MyYesterdayWinLossDataTests))]
        public async Task Handle_MyYesterdayWinLoss_ReturnSuccessResult(CustBotAccount account, string[] parameters, WinLossBase winLoss, string[] messages)
        {
            // Arrange
            var expectedMessage = String.Join(telegramFormatter.NewLine(), messages);
            winLossService.GetWinLoss(account.CustId, account.CustomerLevel, yesterday).Returns(winLoss);

            // Act
            var result = await yesterdayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(0, result.Code);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public async Task Handle_OtherParameters_CallGetDownlineInfo()
        {
            // Arrange
            var account = new CustBotAccount();
            string[] parameters = { "downlineInfo" };

            // Act
            await yesterdayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            await customerService.ReceivedWithAnyArgs().GetDownlineInfo(account.CustId, "downlineInfo");
        }

        [Fact]
        public async Task Handle_InValidDirectDownline_ReturnErrorResult()
        {
            // Arrange
            var account = new CustBotAccount { Language = "en-US", CustId = (byte)CustomerLevel.Super };
            string[] parameters = { "invalidDownlineUsername" };

            customerService.GetDownlineInfo(account.CustId, "invalidDownlineUsername").Returns(Task.FromResult(new DownlineInfo(111, (byte)CustomerLevel.Member, false)));
            var expectedMessage = messageBuilder.BuildInvalidCustNameMessage(account.Language, "invalidDownlineUsername");

            // Act
            var result = await yesterdayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new Result(1, expectedMessage)), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> DownlineYesterdayWinLossDataTests()
        {
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Agent },
                new[] {"validDownlineUsername"},
                new WinLossMember(10, 1, 1, 1, 1, 1, 1, 1, 1),
                new[] { $"[Yesterday Win Loss - {yesterday:yyyyMMdd}]", "<b>USERNAME</b>:", "    validdownlineusername","\n<b>MEMBER WIN LOSS</b>:\n    1.00", "<b>MEMBER COMM</b>:\n    1.00", "<b>MEMBER TOTAL</b>:\n    1.00"}
            };
        }

        [Theory]
        [MemberData(nameof(DownlineYesterdayWinLossDataTests))]
        public async Task Handle_ValidDirectDownline_ReturnSuccessResult(CustBotAccount account, string[] parameters, WinLossBase winLoss, string[] messages)
        {
            // Arrange
            var expectedMessage = String.Join(telegramFormatter.NewLine(), messages);
            customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(new DownlineInfo(111, (byte)(account.CustomerLevel - 1), true)));
            winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, yesterday, 111).Returns(winLoss);

            // Act
            var result = await yesterdayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(0, result.Code);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public async Task GetTopDownlineUsername()
        {
            // Arrange
            var account = new CustBotAccount { Language = "en-US", CustId = 111, CustomerLevel = (byte)CustomerLevel.Master };
            var topDownlineUsername = new[] { "top", "top1", "top3" };
            winLossService.GetTopUsername(account.CustId, account.CustomerLevel, yesterday, 5).Returns(topDownlineUsername);

            // Act
            var result = await yesterdayWinLossFunctionHandler.GetTopDownlineUsername(account, 5);

            // Assert
            Assert.Equal(topDownlineUsername, result);
        }
    }
}