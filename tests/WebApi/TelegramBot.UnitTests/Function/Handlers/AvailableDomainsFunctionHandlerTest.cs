using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Fanex.Data.Repository;
using System.Collections.Generic;
using TelegramBot.Modules.Function.Handlers;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Bot.Models;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Models;
using TelegramBot.Modules.Function.Criteria;
using TelegramBot.Modules.Function.Models;

namespace TelegramBot.Modules.UnitTests.Function.Handlers
{
    public class AvailableDomainsFunctionHandlerTest
    {
        private readonly AvailableDomainsFunctionHandler handler;
        private readonly IMessageBuilder messageBuilder;
        private readonly TelegramFormatter telegramFormatter;
        private readonly IDynamicRepository dynamicRepository;
        private readonly BotEntity bot;

        public AvailableDomainsFunctionHandlerTest()
        {
            messageBuilder = Substitute.For<IMessageBuilder>();
            telegramFormatter = new TelegramFormatter();
            dynamicRepository = Substitute.For<IDynamicRepository>();

            bot = new BotEntity(0, string.Empty, string.Empty, string.Empty, false, false, string.Empty);

            handler = new AvailableDomainsFunctionHandler(messageBuilder, telegramFormatter, dynamicRepository);
        }

        [Fact]
        public async Task GetTopDownlineUsername_Always_ReturnEmpty()
        {
            // Arrange
            var requestAccount = new CustBotAccount
            {
                CustomerLevel = (byte)CustomerLevel.Super,
                CustId = 1,
                Language = "CN"
            };

            // Act
            var actualValue = await handler.GetTopDownlineUsername(requestAccount);


            // Assert
            Assert.Empty(actualValue);
        }

        [Fact]
        public async Task Handle_Always_ReturnDomainList()
        {
            // Arrange
            var requestAccount = new CustBotAccount
            {
                CustomerLevel = (byte)CustomerLevel.Super,
                CustId = 1,
                Language = "CN"
            };

            var param = new[] { "SiteLabel" };
            var expect = new Result(0, "");
            dynamicRepository.FetchAsync<BackupDomainModel>(Arg.Any<GetGetBackupDomainsCriteria>()).Returns(new List<BackupDomainModel> { });

            // Act
            var result = await handler.Handle(requestAccount, param);

            // Assert
            Assert.Equal(expect.Code, result.Code);
            Assert.Equal(expect.Message, result.Message);
        }
    }
}