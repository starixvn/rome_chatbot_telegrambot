using Alpha.API.Client.Services;
using AutoFixture.Xunit2;
using Fanex.Data.Repository;
using NSubstitute;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
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
            _webAPIConfiguration.OtpExpirationInMinutes = 15;
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
        public async Task Start_Always_InsertCustBotAccount(int botId, long chatId, int custId, int subAccId, string language, string ip)
        {
            // Act
            await _settingService.Start(botId, chatId, custId, subAccId, language, ip);

            // Assert
            await _dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotAccountInsCriteria>());
        }

        [Theory, AutoData]
        public async Task SendUnlinkMessage_Always_CallBuildAndSendMessage(int userId, long chatId, int botId, string botName, string language)
        {
            // Assert
            var message = "UnlinkMessage";
            _messageBuilder.BuildUnlinkMessage(language, botName).Returns(message);

            // Act
            await _settingService.SendUnlinkMessage(userId, chatId, botId, botName, language);

            // Assert
            await _messageSender.Received().SendMessage(userId, new Messenger(botId, chatId), message);
        }

        [Theory, AutoData]
        public async Task UpdateUserFunction_Always_Calls(int userid, int botId, BotFunction function, int creator, string ip)
        {
            // Act
            await _settingService.UpdateUserFunction(userid, botId, function, creator, ip);

            // Assert
            await _dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotFunctionUpdCriteria>());
        }

        [Theory, AutoData]
        public async Task UpdateUserLanguage_Always_Calls(int userId, int botId, string language, int creator, string ip)
        {
            // Act
            await _settingService.UpdateUserLanguage(userId, botId, language, creator, ip);

            // Assert
            await _dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotAccountLanguageUpdCriteria>());
        }
    }
}