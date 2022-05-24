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

        // [Fact]
        // public async Task GetAllAsync_Always_Call()
        // {
        //     // Arrange
        //     IEnumerable<BotEntity> botEntities = new List<BotEntity>
        //     {
        //         new BotEntity(1,"Name1","System1","123:AccessToken1",true,false),
        //         new BotEntity(2,"Name2","System2","456:AccessToken2",false,true)
        //     };
        //     _dynamicRepository.FetchAsync<BotEntity>(Arg.Any<GetAllCriteria>()).Returns(Task.FromResult(botEntities));

        //     // Act
        //     var result = await _botService.GetAllAsync().ConfigureAwait(false);

        //     // Assert
        //     var expectedResult = new List<BotEntity>
        //     {
        //         new BotEntity(1,"Name1","System1","123:AccessToken1",true,false)
        //     };
        //     Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        // }
    }
}