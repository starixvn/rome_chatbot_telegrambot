using AutoFixture.Xunit2;
using Newtonsoft.Json;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBot.Client.Models;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.WebApi.Controllers;
using Xunit;

namespace TelegramBot.WebApi.UnitTests.Controllers
{
    [Trait("Controllers", "Bot")]
    public class BotControllerTests
    {
        private readonly IBotService _botService;

        public BotControllerTests()
        {
            _botService = Substitute.For<IBotService>();
        }

        [Theory, AutoData]
        public async Task GetAllAsync_Always_CallsBotServiceGetAllAsync(List<BotEntity> botEntities)
        {
            // Arrange
            var botController = new BotController(_botService);
            _botService.GetAllAsync().Returns(Task.FromResult(botEntities));

            // Act
            var result = await botController.GetAllAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(botEntities), JsonConvert.SerializeObject(result));
        }
    }
}
