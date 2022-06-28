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

namespace TelegramBot.Infrastructure.UnitTests.Function
{
    public class TodayWinLossFunctionHandlerTests
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageFormatter messageFormatter;
        private readonly ICustomerService customerService;
        private readonly IWinLossService winLossService;
        private readonly TodayWinLossFunctionHandler todayWinLossFunctionHandler;
        private static readonly DateTime today = DateTime.Today;

        public TodayWinLossFunctionHandlerTests()
        {
            messageBuilder = Substitute.For<IMessageBuilder>();
            messageFormatter = Substitute.For<IMessageFormatter>();
            customerService = Substitute.For<ICustomerService>();
            winLossService = Substitute.For<IWinLossService>();

            todayWinLossFunctionHandler = new TodayWinLossFunctionHandler(
                messageBuilder,
                messageFormatter,
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
            var result = await todayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new Result(1)), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> MyWinLossDataTests()
        {
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Super },
                new[] {"m"},
                new WinLossMaster(10, 1, 1, 1, 1, 1, 1),
                new[] { $"[Today Win Loss - {today:yyyyMMdd}]", "TURNOVER: 10.00", "GROSS COMM: 1.00", "MASTER TOTAL: 1.00", "SUPER WL: 1.00","SUPER COMM: 1.00", "SUPER TOTAL: 1.00", "COMPANY: 1.00" }
            };
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Master },
                new[] {"m"},
                new WinLossAgent(10, 1, 1, 1, 1, 1, 1),
                new[] { $"[Today Win Loss - {today:yyyyMMdd}]", "TURNOVER: 10.00", "GROSS COMM: 1.00", "AGENT TOTAL: 1.00", "MASTER WL: 1.00", "MASTER COMM: 1.00", "MASTER TOTAL: 1.00", "COMPANY: 1.00" }
            };
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Agent },
                new[] {"m"},
                new WinLossMember(10, 1, 1, 1, 1, 1, 1, 1, 1),
                new[] { $"[Today Win Loss - {today:yyyyMMdd}]", "TURNOVER: 10.00", "GROSS COMM: 1.00", "MEMBER WL: 1.00", "MEMBER COMM: 1.00", "MEMBER TOTAL: 1.00", "AGENT WL: 1.00", "AGENT COMM: 1.00","AGENT TOTAL: 1.00", "COMPANY: 1.00" }
            };
        }

        [Theory]
        [MemberData(nameof(MyWinLossDataTests))]
        public async Task Handle_MyTodayWinLoss_ReturnSuccessResult(CustBotAccount account, string[] parameters, WinLossBase winLoss, string[] messages)
        {
            // Arrange
            var expectedMessage = String.Join(messageFormatter.NewLine(), messages);
            winLossService.GetWinLoss(account.CustId, account.CustomerLevel, today).Returns(winLoss);

            // Act
            var result = await todayWinLossFunctionHandler.Handle(account, parameters);

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
            await todayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            await customerService.Received().GetDownlineInfo(account.CustId, "downlineInfo");
        }

        [Fact]
        public async Task Handle_InValidDirectDownline_ReturnErrorResult()
        {
            // Arrange
            var account = new CustBotAccount { Language = "en-US", CustId = (byte)CustomerLevel.Super };
            string[] parameters = { "invalidDownlineUsername" };

            customerService.GetDownlineInfo(account.CustId, "invalidDownlineUsername").Returns(new DownlineInfo(111, (byte)CustomerLevel.Member, false));
            var expectedMessage = messageBuilder.BuildInvalidCustNameMessage(account.Language, "invalidDownlineUsername");

            // Act
            var result = await todayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new Result(1, expectedMessage)), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> DownlineWinLossDataTests()
        {
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Agent },
                new[] {"validDownlineUsername"},
                new WinLossMember(10, 1, 1, 1, 1, 1, 1, 1, 1),
                new[] { $"[Today Win Loss - {today:yyyyMMdd}]", "USERNAME:", "    validDownlineUsername","MEMBER WL: 1.00", "MEMBER COMM: 1.00", "MEMBER TOTAL: 1.00"}
            };
        }

        [Theory]
        [MemberData(nameof(DownlineWinLossDataTests))]
        public async Task Handle_ValidDirectDownline_ReturnSuccessResult(CustBotAccount account, string[] parameters, WinLossBase winLoss, string[] messages)
        {
            // Arrange
            var expectedMessage = String.Join(messageFormatter.NewLine(), messages);
            customerService.GetDownlineInfo(account.CustId, "validDownlineUsername").Returns(new DownlineInfo(111, (byte)(account.CustomerLevel - 1), true));
            winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, today, 111).Returns(winLoss);

            // Act
            var result = await todayWinLossFunctionHandler.Handle(account, parameters);

            // Assert
            Assert.Equal(0, result.Code);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public async Task GetTopDownlineUsername()
        {
            // Arrange
            var account = new CustBotAccount { Language = "en-US", CustId = 111, CustomerLevel = (byte)CustomerLevel.Master };
            var topDownlineUsername = new[] { "top1", "top2", "top3" };
            winLossService.GetTopUsername(account.CustId, account.CustomerLevel, today, 5).Returns(topDownlineUsername);

            // Act
            var result = await todayWinLossFunctionHandler.GetTopDownlineUsername(account, 5);

            // Assert
            Assert.Equal(topDownlineUsername, result);
        }
    }
}

