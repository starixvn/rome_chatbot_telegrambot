using AutoFixture.Xunit2;
using Newtonsoft.Json;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.WebApi.Controllers;
using Xunit;

namespace TelegramBot.WebApi.UnitTests.Controllers
{
    [Trait("Controllers", "Bot")]
    public class BotControllerTests
    {
        private readonly IBotService _botService;
        private readonly BotController _botController;

        public BotControllerTests()
        {
            _botService = Substitute.For<IBotService>();
            _botController = new BotController(_botService);
        }

        [Theory, AutoData]
        public async Task GetAllAsync_Always_CallsBotServiceGetAllAsync(List<BotEntity> botEntities)
        {
            // Arrange
            _botService.GetAllAsync().Returns(Task.FromResult(botEntities));

            // Act
            var result = await _botController.GetAllBots().ConfigureAwait(false);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(botEntities), JsonConvert.SerializeObject(result));
        }

        [Theory, AutoData]
        public async Task GetBotBySystem(BotEntity bot)
        {
            // Arrange
            var system = "System";
            _botService.GetAsync(system).Returns(Task.FromResult(bot));

            // Act
            var result = await _botController.GetBotBySystem(system);

            // Assert
            Assert.Same(result, bot);
        }
    }
}