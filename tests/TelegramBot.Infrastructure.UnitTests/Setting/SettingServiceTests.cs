using AutoFixture.Xunit2;
using Fanex.Caching;
using Fanex.Data.Repository;
using NSubstitute;
using System.Threading.Tasks;
using TelegramBot.Common.Models;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Setting
{
    public class SettingServiceTests
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IBotService _botService;
        private readonly ICacheService _cacheService;
        private readonly ISenderService _senderService;
        private readonly ISettingService _settingService;

        public SettingServiceTests()
        {
            _webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            _dynamicRepository = Substitute.For<IDynamicRepository>();
            _botService = Substitute.For<IBotService>();
            _cacheService = Substitute.For<ICacheService>();
            _senderService = Substitute.For<ISenderService>();
            _settingService = new SettingService(_webAPIConfiguration, _dynamicRepository, _botService, _cacheService, _senderService);
        }

        [Theory, AutoData]
        public async Task UnlinkAsync_Always_Call(int botId, int custId, int subAccId, string ip)
        {
            // Arrange
            var criteria = new CustBotAccountStatusUpdCriteria(botId, custId, subAccId, false, false, ip);
            _dynamicRepository.ExecuteAsync(criteria).Returns(Task.FromResult(0));

            // Act
            await _settingService.UnlinkAsync(botId, custId, subAccId, ip).ConfigureAwait(false);

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
            await _settingService.GetCustBotMessengerAsync(custId, subAccId).ConfigureAwait(false);

            // Assert
            await _dynamicRepository.Received().GetAsync<CustBotMessenger>(Arg.Any<CustBotMessengerSelCriteria>());
        }
    }
}