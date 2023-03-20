using Alpha.API.Client.Models.Outstanding;
using TelegramBot.Modules.Message;
using TelegramBot.Shared.Configurations;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Extensions;
using TelegramBot.Shared.Models;
using TelegramBot.Shared.Resources;
using TelegramBot.Modules.Function.Handlers;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Alpha.API.Client.Services;
using TelegramBot.Modules.Setting.Models;
using System.Collections.Generic;
using Alpha.API.Client.Models.BalanceInfo;
using System;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Utilities.Extensions;

namespace TelegramBot.Modules.UnitTests.Function.Handlers
{
    public class BalanceInfoFunctionHandlerTest
    {
        private readonly BalanceInfoFunctionHandler handler;
        private readonly IMessageBuilder messageBuilder;
        private readonly TelegramFormatter telegramFormatter;
        private readonly ICustomerService customerService;
        private readonly IBalanceInfoService balanceInfoService;


        private readonly WebAPIConfiguration webAPIConfiguration;

        public BalanceInfoFunctionHandlerTest()
        {
            customerService = Substitute.For<ICustomerService>();
            balanceInfoService = Substitute.For<IBalanceInfoService>();
            webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            messageBuilder = Substitute.For<IMessageBuilder>();
            telegramFormatter = new TelegramFormatter();

            handler = new BalanceInfoFunctionHandler(messageBuilder, telegramFormatter, customerService, balanceInfoService, webAPIConfiguration);

        }

        public static Dictionary<string, string> BuildMasterDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}", bal.TotalCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}", bal.YesterdayCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.BAL), lang)}", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}", bal.Outstanding.MasterOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.UsedCredit),lang)}", bal.UsedCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}", bal.Outstanding.MemberOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.SuperOutStanding), lang)}", bal.Outstanding.SuperOutstanding.ToFormattedNumber()}
            };
        }

        public static Dictionary<string, string> BuildAgentDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}", bal.TotalCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}", bal.YesterdayCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.BAL), lang)}", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}", bal.Outstanding.AgentOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.UsedCredit),lang)}", bal.UsedCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemMaxCredit),lang)}", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}", bal.Outstanding.MemberOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}", bal.Outstanding.MasterOutstanding.ToFormattedNumber()}
            };
        }

        public static Dictionary<string, string> BuildMemberDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.BAL), lang)}", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.NetPosition), lang)}", bal.NetPosition?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.BetCredit), lang)}", bal.BetCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}", bal.Outstanding.MemberOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}", bal.Outstanding.AgentOutstanding.ToFormattedNumber()}
            };
        }

        [Fact]
        public async Task Handle_ValidRequestMyBalance_ReturnSuccessMessage()
        {
            // Arrange
            var request = new CustBotAccount
            {
                CustomerLevel = 2,
                CustId = 1,
                Language = "CN"
            };
            var parameters = new[] { "m" };
            var lang = request.Language;
            var bal = new CustomerBalance("USD", 1, 2, 3, 4, 5, 6, 7, 8, 9, new CustomerOutstanding(10, 11, 12, 13));
            balanceInfoService.GetMyBalance(request.CustId, request.CustomerLevel).Returns(bal);

            var dict = new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Currency), lang)}", bal.CurrencyName},
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}", bal.CashBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}", bal.YesterdayCashBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.BAL), lang)}", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}", bal.YesterdayTotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), lang)}", bal.TodayWinLoss.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayWinLoss), lang)}", bal.YesterdayWinLoss.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get($"{(CustomerLevel)request.CustomerLevel - 1}Credit",lang)}", bal.MemberCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}", bal.Outstanding.MemberOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get($"{(CustomerLevel)request.CustomerLevel}OutStanding", lang)}", bal.Outstanding.AgentOutstanding.ToFormattedNumber()},
            };
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.YourBalance), lang)} - {DateTime.Now.ToSimpleDate()}]",
                "<pre>"
            };
            foreach (var keyValuePair in dict)
            {
                textContainer.Add($"{keyValuePair.Key,-15} {keyValuePair.Value,16}");
            }
            textContainer.Add("</pre>");
            var message = string.Join("\n", textContainer);

            var expect = new Result(0, message);

            // Act
            var result = await handler.Handle(request, parameters);

            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
        }

        public static IEnumerable<object[]> RequestsGetDownLineUserName()
        {
            yield return new object[] { new CustBotAccount() { CustomerLevel = (byte)CustomerLevel.Super, CustId = 1, Language = "CN" }, new[] { "Username " } };
            yield return new object[] { new CustBotAccount() { CustomerLevel = (byte)CustomerLevel.Master, CustId = 1, Language = "CN" }, new[] { "Username " } };
            yield return new object[] { new CustBotAccount() { CustomerLevel = (byte)CustomerLevel.Agent, CustId = 1, Language = "CN" }, new[] { "Username " } };
        }

        [Theory]
        [MemberData(nameof(RequestsGetDownLineUserName))]
        public async Task Handle_ValidRequestDownLineUserName_ReturnSuccessMessage(CustBotAccount requestAccount, string[] parameters)
        {
            // Arrange
            var downlineInfo = new DownlineInfo(1, (byte)(requestAccount.CustomerLevel - 1), true);
            customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(downlineInfo));
            var downlineBalance = new DownlineBalance(1, 2, 3, 4, 5, 6, 7, 8, new List<TotalDownline>(), 9, 10,
                new CustomerOutstanding(11, 12, 13, 14));
            balanceInfoService.GetDownlineBalance(Arg.Any<int>(), Arg.Any<byte>()).Returns(downlineBalance);

            var textContainer = new List<string>()
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineBalance), requestAccount.Language)} - {DateTime.Now.ToSimpleDate()}]",
                "<pre>",
                $"{ResourceHelper.Get(nameof(SharedResource.Username), requestAccount.Language),-15} {parameters[0].ToLowerInvariant(),16}"
            };
            var dict = (CustomerLevel)downlineInfo.RoleId switch
            {
                CustomerLevel.Member => BuildMemberDownLineUsernameMessage(downlineBalance, requestAccount.Language),
                CustomerLevel.Agent => BuildAgentDownLineUsernameMessage(downlineBalance, requestAccount.Language),
                CustomerLevel.Master => BuildMasterDownLineUsernameMessage(downlineBalance, requestAccount.Language),
                _ => new Dictionary<string, string>(),
            };

            foreach (var keyValuePair in dict)
            {
                textContainer.Add($"{keyValuePair.Key,-15} {keyValuePair.Value,16}");
            }

            textContainer.Add("</pre>");

            var message = string.Join("\n", textContainer);
            var expect = new Result(0, message);

            // Act
            var result = await handler.Handle(requestAccount, parameters);


            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
        }


        [Fact]
        public async Task Handle_InValidRequestDownLineUserName_ReturnErrorMessage()
        {
            // Arrange
            var requestAccount = new CustBotAccount
            {
                CustomerLevel = (byte)CustomerLevel.Super,
                CustId = 1,
                Language = "CN"
            };


            var downlineInfo = new DownlineInfo(1, (byte)CustomerLevel.Super, true);
            customerService.GetDownlineInfo(Arg.Any<int>(), Arg.Any<string>()).Returns(Task.FromResult(downlineInfo));
            var downlineBalance = new DownlineBalance(1, 2, 3, 4, 5, 6, 7, 8, new List<TotalDownline>(), 9, 10,
                new CustomerOutstanding(11, 12, 13, 14));
            balanceInfoService.GetDownlineBalance(Arg.Any<int>(), Arg.Any<byte>()).Returns(downlineBalance);

            // Act
            var result = await handler.Handle(requestAccount, new[] { "Username" });

            // Assert
            Assert.Equal(1, result.Code);
            Assert.Equal("", result.Message);
        }

        public static IEnumerable<object[]> RequestsRequestTopXBalance()
        {
            yield return new object[] { new CustBotAccount() { CustomerLevel = (byte)CustomerLevel.Super, CustId = 1, Language = "CN" }, new[] { "10" } };
        }

        [Theory]
        [MemberData(nameof(RequestsRequestTopXBalance))]
        public async Task Handle_ValidRequestTopXBalance_ReturnSuccessMessage(CustBotAccount requestAccount, string[] parameters)
        {
            // Arrange
            var topBalanceReport = new List<TopBalance>()
            {
                new TopBalance(  "UserName", 1, 10 ),
                new TopBalance(  "UserName2", 2, 20 )
            };
            balanceInfoService.GetTopBalance(requestAccount.CustId, requestAccount.CustomerLevel, Arg.Any<int>()).Returns(topBalanceReport);
            webAPIConfiguration.BalanceMaxTopQuery = 100;

            var textContainer = new List<string>()
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineTopHighestBalance), requestAccount.Language)} - {DateTime.Now.ToSimpleDate()}]",
                "<pre>",
                $"{ResourceHelper.Get(nameof(SharedResource.Username), requestAccount.Language),-15} {ResourceHelper.Get(nameof(SharedResource.Today), requestAccount.Language),16}",
                "________________________________"
            };
            foreach (var item in topBalanceReport)
            {
                textContainer.Add($"{item.Username,-15} {item.Balance.ToFormattedNumber(),16}");
            }
            textContainer.Add("</pre>");

            var message = string.Join(telegramFormatter.NewLine(), textContainer);
            var expect = new Result(0, message);

            // Act
            var result = await handler.Handle(requestAccount, parameters);

            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
        }


        public static IEnumerable<object[]> RequestTopXBalanceOutOfRange()
        {
            yield return new object[] { new CustBotAccount() { CustomerLevel = (byte)CustomerLevel.Super, CustId = 1, Language = "CN" }, new[] { "101" } };
            yield return new object[] { new CustBotAccount() { CustomerLevel = (byte)CustomerLevel.Super, CustId = 1, Language = "CN" }, new[] { "-1" } };
        }

        [Theory]
        [MemberData(nameof(RequestTopXBalanceOutOfRange))]
        public async Task Handle_RequestTopXBalanceOutOfRange_ReturnErrorResultMessage(CustBotAccount requestAccount, string[] parameters)
        {
            // Arrange
            var topBalanceReport = new List<TopBalance>()
            {
                new TopBalance(  "DirectDownLineUserName", 1, 10 ),
                new TopBalance(  "DirectDownLineUserName2", 2, 20 )
            };
            balanceInfoService.GetTopBalance(requestAccount.CustId, requestAccount.CustomerLevel, Arg.Any<int>()).Returns(topBalanceReport);
            webAPIConfiguration.BalanceMaxTopQuery = 100;
            messageBuilder.BuildInvalidNumberOfDownlineRange(requestAccount.Language, parameters[0], webAPIConfiguration.BalanceMaxTopQuery).Returns("MessageAlert");


            var expect = new Result(1, "");

            // Act
            var result = await handler.Handle(requestAccount, parameters);

            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);

        }

        [Fact]
        public async Task Handle_NullParameters_ReturnErrorResult()
        {
            // Arrange
            var account = new CustBotAccount();
            string[] parameters = { "" };

            // Act
            var result = await handler.Handle(account, parameters);

            // Assert
            Assert.Equal(1, result.Code);
            Assert.Equal("", result.Message);
        }
        [Fact]
        public async Task GetTopDownlineUsername()
        {
            // Arrange
            var account = new CustBotAccount { Language = "en-US", CustId = 111, CustomerLevel = (byte)CustomerLevel.Master };
            var topDownlineUsername = new[] { "top1", "top2", "top3" };
            balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel).Returns(topDownlineUsername);

            // Act
            var result = await handler.GetTopDownlineUsername(account, 5);

            // Assert
            Assert.Equal(topDownlineUsername, result);
        }
    }
}