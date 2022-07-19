using Fanex.Data.Repository;
using Newtonsoft.Json;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Bot.Criteria;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Bot
{
    public class BotServiceTests
    {
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IBotService _botService;
        private readonly WebAPIConfiguration _webAPIConfiguration;

        public BotServiceTests()
        {
            _dynamicRepository = Substitute.For<IDynamicRepository>();
            _webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            _botService = new BotService(_dynamicRepository, _webAPIConfiguration);
        }

        [Fact]
        public async Task GetAllAsync_Always_ReturnAllBot()
        {
            // Arrange
            IEnumerable<BotEntity> botEntities = new List<BotEntity>
            {
                new BotEntity(1, "Name1", "System1", "123:AccessToken1", true, false, "Name1"),
                new BotEntity(2, "Name2", "System2", "456:AccessToken2", false, true, "Name2")
            };
            _dynamicRepository.FetchAsync<BotEntity>(Arg.Any<GetAllCriteria>()).Returns(Task.FromResult(botEntities));

            // Act
            var result = await _botService.GetAllAsync().ConfigureAwait(false);

            // Assert
            var expectedResult = new List<BotEntity>
            {
                new BotEntity(1, "Name1", "System1", "123:AccessToken1", true, false, "Name1") { MessengerBotId = 123},
                new BotEntity(2, "Name2", "System2", "456:AccessToken2", false, true, "Name2") { MessengerBotId = 456}
            };
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }

        public static IEnumerable<object[]> BotDataTests()
        {
            var data = new List<BotEntity>
            {
                new BotEntity(1, "Name1", "System1", "123:AccessToken1", true, false, "Name1"),
                new BotEntity(2, "Name2", "System2", "456:AccessToken2", false, true, "Name2")
            };
            yield return new object[] { data, "System1", new BotEntity(1, "Name1", "System1", "123:AccessToken1", true, false, "Name1") { MessengerBotId = 123 } };
            yield return new object[] { data, (long)456, new BotEntity(2, "Name2", "System2", "456:AccessToken2", false, true, "Name2") { MessengerBotId = 456 } };
            yield return new object[] { data, 1, new BotEntity(1, "Name1", "System1", "123:AccessToken1", true, false, "Name1") { MessengerBotId = 123 } };
        }

        [Theory]
        [MemberData(nameof(BotDataTests))]
        public async Task GetAsync_Always_ReturnCorrectBot(IEnumerable<BotEntity> bots, dynamic parameter, BotEntity expectedBot)
        {
            // Arrange
            _dynamicRepository.FetchAsync<BotEntity>(Arg.Any<GetAllCriteria>()).Returns(Task.FromResult(bots));

            // Act
            var result = await _botService.GetAsync(parameter);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedBot), JsonConvert.SerializeObject(result));
        }
    }
}