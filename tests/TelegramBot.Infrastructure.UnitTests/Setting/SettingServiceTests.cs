using AutoFixture.Xunit2;
using Fanex.Caching;
using Fanex.Data.Repository;
using NSubstitute;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Setting
{
    public class SettingServiceTests
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly ICacheService _cacheService;
        private readonly ISenderService _senderService;
        private readonly ISettingService _settingService;
        private readonly IBuilderService _builderService;
        private readonly IActionService _actionService;

        public SettingServiceTests()
        {
            _webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            _dynamicRepository = Substitute.For<IDynamicRepository>();
            _cacheService = Substitute.For<ICacheService>();
            _senderService = Substitute.For<ISenderService>();
            _builderService = Substitute.For<IBuilderService>();
            _actionService = Substitute.For<IActionService>();
            _settingService = new SettingService(_webAPIConfiguration, _dynamicRepository, _cacheService, _senderService, _builderService, _actionService);
        }

        [Theory, AutoData]
        public async Task UnlinkAsync_Always_Call(int botId, int custId, int subAccId, string ip)
        {
            // Arrange
            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, false, false, ip);
            _dynamicRepository.ExecuteAsync(criteria).Returns(Task.FromResult(0));

            // Act
            await _settingService.Unlink(botId, custId, subAccId, ip).ConfigureAwait(false);

            // Assert
            await _dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotAccountStatusUpdCriteria>());
        }

        [Theory, AutoData]
        public async Task GetCustBotMessengerAsync_Always_Call(int custId, int subAccId, bool isTest, CustBotMessenger custBotMessenger)
        {
            // Arrange
            var criteria = new CustBotMessengerSelCriteria(custId, subAccId, isTest);
            _dynamicRepository.GetAsync<CustBotMessenger>(criteria).Returns(Task.FromResult(custBotMessenger));

            // Act
            await _settingService.GetCustBotMessenger(custId, subAccId).ConfigureAwait(false);

            // Assert
            await _dynamicRepository.Received().GetAsync<CustBotMessenger>(Arg.Any<CustBotMessengerSelCriteria>());
        }
    }
}