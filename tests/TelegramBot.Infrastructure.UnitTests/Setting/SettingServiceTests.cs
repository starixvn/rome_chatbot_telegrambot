using Alpha.API.Client.Services;
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
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Setting
{
    public class SettingServiceTests
    {
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IMessageSender _messageSender;
        private readonly ISettingService _settingService;
        private readonly IMessageBuilder _messageBuilder;
        private readonly IActionService _actionService;
        private readonly ICustomerService _customerService;
        private readonly IFunctionService _functionService;

        public SettingServiceTests()
        {
            _webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            _dynamicRepository = Substitute.For<IDynamicRepository>();
            _messageSender = Substitute.For<IMessageSender>();
            _messageBuilder = Substitute.For<IMessageBuilder>();
            _actionService = Substitute.For<IActionService>();
            _customerService = Substitute.For<ICustomerService>();
            _functionService = Substitute.For<IFunctionService>();
            _settingService = new SettingService(
                _webAPIConfiguration,
                _dynamicRepository,
                Substitute.For<ICacheServiceFactory>(),
                _messageSender,
                _messageBuilder,
                _actionService,
                _customerService,
                _functionService);
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