using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.UnitTests.Function.Handlers
{
    using TelegramBot.Infrastructure.Function.Handlers;
    using System.Threading.Tasks;
    using Xunit;
    using Message;
    using NSubstitute;
    using Alpha.API.Client.Services;
    using TelegramBot.Infrastructure.Setting.Models;
    using Shared.Models;
    using System.Collections.Generic;
    using Alpha.API.Client.Models.BalanceInfo;
    using System;
    using Shared.Constants;
    using Shared.Extensions;
    using Shared.Resources;
    using Alpha.API.Client.Models.Customers;
    using System.Linq;

    public class BalanceInfoFunctionHandlerUnitTest
    {
        private readonly BalanceInfoFunctionHandler handler;
        private readonly IMessageBuilder messageBuilder;
        private readonly TelegramFormatter telegramFormatter;
        private readonly ICustomerService customerService;
        private readonly IBalanceInfoService balanceInfoService;


        private readonly WebAPIConfiguration webAPIConfiguration;

        public BalanceInfoFunctionHandlerUnitTest()
        {
            customerService = Substitute.For<ICustomerService>();
            balanceInfoService = Substitute.For<IBalanceInfoService>();
            webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            messageBuilder = Substitute.For<IMessageBuilder>();
            telegramFormatter = new TelegramFormatter();
            
            handler = new BalanceInfoFunctionHandler(messageBuilder, telegramFormatter, customerService, balanceInfoService, webAPIConfiguration);

        }

        private string BuildTotalACT_CLOSED_SUS(IEnumerable<TotalDownline> totalDownline, CustomerLevel level)
        {
            var downline = totalDownline.ToList().Find(e => e.RoleId == (byte)level);
            return downline == null ? "0/0/0" : $"{downline.TotalActive}/{downline.TotalClosed}/{downline.TotalSuspended}";
        }
        public Dictionary<string, string> BuildMasterDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.TotalCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:", bal.YesterdayCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:", bal.Outstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.UsedCredit),lang)}:", bal.UsedCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentActClosedSus), lang)}:", $"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines,CustomerLevel.Agent)}"},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberActClosedSus), lang)}:", $"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines,CustomerLevel.Member)}"},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.TotalOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.SuperOutStanding), lang)}:", bal.UplineOutstanding.ToFormattedNumber()}
            };
        }

        public Dictionary<string, string> BuildAgentDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.TotalCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:", bal.YesterdayCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:", bal.Outstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.UsedCredit),lang)}:", bal.UsedCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemMaxCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberActClosedSus), lang)}:",$"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines,CustomerLevel.Member)}"},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.TotalOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:", bal.UplineOutstanding.ToFormattedNumber()}
            };
        }

        public Dictionary<string, string> BuildMemberDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.NetPosition), lang)}:", bal.NetPosition?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.BetCredit), lang)}:", bal.BetCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.Outstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:", bal.UplineOutstanding.ToFormattedNumber()}
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
            var bal = new CustomerBalance("USD", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            balanceInfoService.GetMyBalance(request.CustId, request.CustomerLevel).Returns(bal);

            var dict = new Dictionary<string, string>
            {
                {$"[{ResourceHelper.Get(nameof(SharedResource.Balance), lang)} - {DateTime.Now:yyyyMMdd}]",""},
                {$"{ResourceHelper.Get(nameof(SharedResource.Currency), lang)}:", bal.CurrencyName},
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.CashBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:", bal.YesterdayTotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), lang)}:", bal.TodayWinLoss.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayWinLoss), lang)}:", bal.YesterdayWinLoss.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get($"{(CustomerLevel)request.CustomerLevel - 1}Credit",lang)}:", bal.MemberCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.TotalOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get($"{(CustomerLevel)request.CustomerLevel}OutStanding", lang)}:", bal.Outstanding.ToFormattedNumber()},
            };
            var textContainer = new List<string>();
            foreach (var keyValuePair in dict)
            {
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }
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
            var downlineBalance = new DownlineBalance(1, 2, 3, 4, 5, 6, 7, 8, 9, new TotalDownline[] { }, 10, 11, 12, 13);
            balanceInfoService.GetDownlineBalance(Arg.Any<int>(), Arg.Any<byte>(), Arg.Any<int>()).Returns(downlineBalance);

            var textContainer = new List<string>()
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.Balance),  requestAccount.Language)} - {DateTime.Now:yyyyMMdd}]",
                $"{ResourceHelper.Get(nameof(SharedResource.Username), requestAccount.Language).ToUpperInvariant()}:",
                $"{parameters[0]}\n"
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
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }

            var message = string.Join("\n", textContainer);
            var expect = new Result(0, message);

            // Act
            var result = await handler.Handle(requestAccount, parameters);


            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
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
                new TopBalance(  "DirectDownLineUserName", 1, 10 ),
                new TopBalance(  "DirectDownLineUserName2", 2, 20 )
            };
            balanceInfoService.GetTopBalance(requestAccount.CustId, requestAccount.CustomerLevel, Arg.Any<int>()).Returns(topBalanceReport);
            webAPIConfiguration.BalanceMaxTopQuery = 100;

            var textContainer = new List<string>()
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.Balance), requestAccount.Language)} - {DateTime.Now:yyyyMMdd}]",
                ResourceHelper.Get($"CustLevel_{requestAccount.CustomerLevel - 1}", requestAccount.Language),
                $"{ResourceHelper.Get(nameof(SharedResource.Today), requestAccount.Language)}{telegramFormatter.Point()}{ResourceHelper.Get(nameof(SharedResource.Yesterday), requestAccount.Language)}{telegramFormatter.NewLine()}",

            };
            var index = 1;
            foreach (var item in topBalanceReport)
            {
                textContainer.Add($"{index++}.{item.Username}:");
                textContainer.Add($"    {item.Balance.ToFormattedNumber()}{telegramFormatter.Point()}{item.YesterdayBalance.ToFormattedNumber()}");
            }

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
            yield return new object[] { new CustBotAccount(){CustomerLevel = (byte)CustomerLevel.Super, CustId = 1, Language = "CN"}, new []{"101"} };
        }
        
        [Theory]
        [MemberData(nameof(RequestTopXBalanceOutOfRange))]
        public async Task Handle_RequestTopXBalanceOutOfRange_ReturnSuccessMessage(CustBotAccount requestAccount, string[] parameters)
        {
            // Arrange
            var topBalanceReport = new List<TopBalance>()
            {
                new TopBalance(  "DirectDownLineUserName", 1, 10 ),
                new TopBalance(  "DirectDownLineUserName2", 2, 20 )
            };
            balanceInfoService.GetTopBalance(requestAccount.CustId, requestAccount.CustomerLevel, Arg.Any<int>()).Returns(topBalanceReport);
            webAPIConfiguration.BalanceMaxTopQuery = 100;
            messageBuilder.BuildInvalidNumerOfDownlineRange(requestAccount.Language, parameters[0], webAPIConfiguration.BalanceMaxTopQuery).Returns("MessageAlert");

            
          //  var message = string.Join(telegramFormatter.NewLine(), textContainer);
            var expect = new Result(1, "");

            // Act
            var result = await handler.Handle(requestAccount, parameters);

            // Assert
            Assert.Equal( expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
          
        }
    }
}