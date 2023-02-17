using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Alpha.API.Client.Services;
using Alpha.API.Client.Models.WinLoss;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Modules.Function.Handlers;
using TelegramBot.Shared.Models;
using Newtonsoft.Json;
using Xunit;
using NSubstitute;

namespace TelegramBot.Modules.UnitTests.Function
{
    public class TodayWinLossFunctionHandlerTests
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly TelegramFormatter telegramFormatter;
        private readonly ICustomerService customerService;
        private readonly IWinLossService winLossService;
        private readonly TodayWinLossFunctionHandler todayWinLossFunctionHandler;
        private static readonly DateTime today = DateTime.Today;

        public TodayWinLossFunctionHandlerTests()
        {
            messageBuilder = Substitute.For<IMessageBuilder>();
            telegramFormatter = Substitute.For<TelegramFormatter>();
            customerService = Substitute.For<ICustomerService>();
            winLossService = Substitute.For<IWinLossService>();

            todayWinLossFunctionHandler = new TodayWinLossFunctionHandler(
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
            var result = await todayWinLossFunctionHandler.Handle(account, parameters, null);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new Result(1)), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> MyWinLossDataTests()
        {
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Super },
                new[] {"m"},
                new WinLossMaster(10, 1, 1, 1, 1, 1, 1),
                new[] {
                    $"[Your Today Win Loss - {today:MM/dd/yyyy}]",
                    "<pre>",
                    $"{"TO",-15} {"10",16}",
                    $"{"Gross Comm",-15} {"1",16}",
                    $"{"Master Total",-15} {"1",16}",
                    $"{"Super WL",-15} {"1",16}",
                    $"{"Super Comm",-15} {"1",16}",
                    $"{"Super Total",-15} {"1",16}",
                    $"{"Company",-15} {"1",16}",
                    "</pre>"
                }
            };
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Master },
                new[] {"m"},
                new WinLossAgent(10, 1, 1, 1, 1, 1, 1),
                new[] {
                    $"[Your Today Win Loss - {today:MM/dd/yyyy}]",
                    "<pre>",
                    $"{"TO",-15} {"10",16}",
                    $"{"Gross Comm",-15} {"1",16}",
                    $"{"Agent Total",-15} {"1",16}",
                    $"{"Master WL",-15} {"1",16}",
                    $"{"Master Comm",-15} {"1",16}",
                    $"{"Master Total",-15} {"1",16}",
                    $"{"Company",-15} {"1",16}",
                    "</pre>"
                },
            };
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Agent },
                new[] {"m"},
                new WinLossMember(10, 1, 1, 1, 1, 1, 1, 1, 1),
                new[] {
                    $"[Your Today Win Loss - {today:MM/dd/yyyy}]",
                    "<pre>",
                    $"{"TO",-15} {"10",16}",
                    $"{"Gross Comm",-15} {"1",16}",
                    $"{"Member WL",-15} {"1",16}",
                    $"{"Member Comm",-15} {"1",16}",
                    $"{"Member Total",-15} {"1",16}",
                    $"{"Agent WL",-15} {"1",16}",
                    $"{"Agent Comm",-15} {"1",16}",
                    $"{"Agent Total",-15} {"1",16}",
                    $"{"Company",-15} {"1",16}",
                    "</pre>"
                },
            };
        }

        [Theory]
        [MemberData(nameof(MyWinLossDataTests))]
        public async Task Handle_MyTodayWinLoss_ReturnSuccessResult(CustBotAccount account, string[] parameters, WinLossBase winLoss, string[] messages)
        {
            // Arrange
            var expectedMessage = string.Join(telegramFormatter.NewLine(), messages);
            winLossService.GetWinLoss(account.CustId, account.CustomerLevel, today).Returns(winLoss);

            // Act
            var result = await todayWinLossFunctionHandler.Handle(account, parameters, null);

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
            await todayWinLossFunctionHandler.Handle(account, parameters, null);

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
            var result = await todayWinLossFunctionHandler.Handle(account, parameters, null);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(new Result(1, expectedMessage)), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> DownlineWinLossDataTests()
        {
            yield return new object[] {
                new CustBotAccount { Language = "en-US", CustId = 123, CustomerLevel = (byte)CustomerLevel.Agent },
                new[] {"Username"},
                new WinLossMember(10, 1, 1, 1, 1, 1, 1, 1, 1),
                new[] {
                    $"[Downline's Today Win Loss - {today:MM/dd/yyyy}]",
                    "<pre>",
                    $"{"Username",-15} {"username",16}",
                    $"{"Member WL",-15} {"1",16}",
                    $"{"Member Comm",-15} {"1",16}",
                    $"{"Member Total",-15} {"1",16}",
                    "</pre>",
                },
            };
        }


        [Theory]
        [MemberData(nameof(DownlineWinLossDataTests))]
        public async Task Handle_ValidDirectDownline_ReturnSuccessResult(CustBotAccount account, string[] parameters, WinLossBase winLoss, string[] messages)
        {
            // Arrange
            var expectedMessage = string.Join(telegramFormatter.NewLine(), messages);
            customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(new DownlineInfo(111, (byte)(account.CustomerLevel - 1), true)));
            winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, today, 111).Returns(winLoss);

            // Act
            var result = await todayWinLossFunctionHandler.Handle(account, parameters, null);

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

