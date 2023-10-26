using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using TelegramBot.Modules.Function.Handlers;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Models;
using Alpha.API.Client.Services;
using System.Linq;
using System;
using System.Collections.Generic;
using Alpha.API.Client.Models.Customers;

namespace TelegramBot.Modules.UnitTests.Function.Handlers
{
    public class AvailableDomainsFunctionHandlerTest
    {
        private readonly AvailableDomainsFunctionHandler handler;
        private readonly IMessageBuilder messageBuilder;
        private readonly TelegramFormatter telegramFormatter;
        private readonly ICustomerService customerService;

        public AvailableDomainsFunctionHandlerTest()
        {
            messageBuilder = Substitute.For<IMessageBuilder>();
            telegramFormatter = new TelegramFormatter();
            customerService = Substitute.For<ICustomerService>();

            handler = new AvailableDomainsFunctionHandler(messageBuilder, telegramFormatter, customerService);
        }

        [Fact]
        public async Task GetTopDownlineUsername_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var requestAccount = new CustBotAccount
            {
                CustomerLevel = (byte)CustomerLevel.Super,
                CustId = 1,
                Language = "CN"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => handler.GetTopDownlineUsername(requestAccount));
        }

        [Fact]
        public async Task Handle_NoData_ReturnsEmptyString()
        {
            // Arrange
            var requestAccount = new CustBotAccount
            {
                CustomerLevel = (byte)CustomerLevel.Super,
                CustId = 1,
                Language = "en-US"
            };

            var expect = new Result(0, "No information is available");
            customerService.GetDomains(Arg.Any<string>()).Returns(Task.FromResult(Enumerable.Empty<DomainInfo>()));
            messageBuilder.BuildNoDataMessage("en-US").Returns("No information is available");

            // Act
            var result = await handler.Handle(requestAccount, null);

            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
        }

        [Fact]
        public async Task Handle_Always_ReturnsDomainList()
        {
            // Arrange
            var requestAccount = new CustBotAccount
            {
                CustomerLevel = (byte)CustomerLevel.Super,
                CustId = 1,
                Language = "en-US"
            };

            var expect = new Result(0, "www.google.com\nwww.yahoo.com");
            customerService
                .GetDomains(Arg.Any<string>())
                .Returns(Task.FromResult<IEnumerable<DomainInfo>>(new List<DomainInfo>
                {
                    new DomainInfo("www.google.com", true),
                    new DomainInfo("www.yahoo.com", true),
                    new DomainInfo("www.youtube.com", false)
                }));

            // Act
            var result = await handler.Handle(requestAccount, null);

            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
        }
    }
}