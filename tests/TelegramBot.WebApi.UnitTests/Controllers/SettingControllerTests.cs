using AutoFixture.Xunit2;
using Newtonsoft.Json;
using NSubstitute;
using System.Threading.Tasks;
using TelegramBot.Client.Models;
using TelegramBot.Common.Services;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.WebApi.Controllers;
using TelegramBot.WebApi.Params;
using Xunit;

namespace TelegramBot.WebApi.UnitTests.Controllers
{
    [Trait("Controllers", "Setting")]
    public class SettingControllerTests
    {
        private readonly ICryptography _cryptography;
        private readonly ISettingService _settingService;
        private readonly IBotService _botService;
        private readonly SettingController _settingController;
        private readonly ISenderService _senderService;

        public SettingControllerTests()
        {
            _cryptography = Substitute.For<ICryptography>();
            _settingService = Substitute.For<ISettingService>();
            _botService = Substitute.For<IBotService>();
            _senderService = Substitute.For<ISenderService>();
            _settingController = new SettingController(_cryptography, _settingService, _botService, _senderService);
        }

        [Theory, AutoData]
        public async Task GenerateStartUriAsync_Always_Calls(GenerateStartUriParam param, BotEntity botEntity)
        {
            // Arrange
            var plainKey = $"{param.CustId}:{param.SubAccId}";
            var encryptKey = "encryptKey";
            _cryptography.Encrypt(plainKey).Returns(encryptKey);
            _botService.GetAsync(param.System, param.IsTest).Returns(Task.FromResult(botEntity));

            // Act
            var result = await _settingController.GenerateStartUriAsync(param).ConfigureAwait(false);

            // Assert
            _cryptography.Received().Encrypt(plainKey);
            await _botService.Received().GetAsync(param.System, param.IsTest);
            var expectedResult = $"https://t.me/{botEntity.Name}?start={encryptKey}";
            Assert.Equal(expectedResult, result);
        }

        [Theory, AutoData]
        public async Task LinkAsync_Always_Calls(LinkParam param, Result expectedResult)
        {
            // Arrange
            _settingService.LinkAsync(param.BotId, param.CustId, param.SubAccId, param.Otp, param.IP, param.Reset)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _settingController.LinkAsync(param).ConfigureAwait(false);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }

        [Theory, AutoData]
        public async Task UnlinkAsync_Always_Calls(UnlinkParam param, Result expectedResult)
        {
            // Arrange
            _settingService.UnlinkAsync(param.BotId, param.CustId, param.SubAccId, param.IP)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _settingController.UnlinkAsync(param).ConfigureAwait(false);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }

        [Theory, AutoData]
        public async Task GetCustBotMessengerAsync_Always_Calls(GetCustBotMessengerParam param, CustBotMessenger custBotMessenger)
        {
            // Arrange
            _settingService.GetCustBotMessengerAsync(param.CustId, param.SubAccId, param.IsTest)
                .Returns(Task.FromResult(custBotMessenger));

            // Act
            var result = await _settingController.GetCustBotMessengerAsync(param).ConfigureAwait(false);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(custBotMessenger), JsonConvert.SerializeObject(result));
        }
    }
}
