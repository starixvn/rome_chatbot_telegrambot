using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
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

        [Fact]
        public void GetAllAsync_Always_CallsBotServiceGetAllAsync()
        {
            _botService.GetAllAsync();
        }
    }
}
