using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Services;
using AutoFixture;
using AutoFixture.Xunit2;
using Fanex.Caching;
using Fanex.Data.Repository;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Constants;
using TelegramBot.Infrastructure.Setting.Criteria;
using TelegramBot.Infrastructure.Setting.Models;
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
        private readonly ICacheService _cacheService;

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
            _cacheService = Substitute.For<ICacheService>();
            var cacheServiceFactory = Substitute.For<ICacheServiceFactory>();
            cacheServiceFactory.Get(Arg.Any<string>()).Returns(_cacheService);
            _settingService = new SettingService(
                _webAPIConfiguration,
                _dynamicRepository,
                cacheServiceFactory,
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

        [Theory, AutoData]
        public async Task ResendOtp_CouldNotFoundMessenger_ReturnsMessagerNotFoundResult(int custId, int subAccId)
        {
            // Assert
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((Enumerable.Empty<CustBotMessenger>(), Enumerable.Empty<BotFunction>())));

            // Act
            var result = await _settingService.ResendOtp(custId, subAccId);

            // Act
            Assert.True(result.Code == ErrorCodes.MessengerNotFound && result.Message == "Messenger not found");
        }

        [Theory, AutoData]
        public async Task ResendOtp_MessengerFounded_ReturnsSuccessfulResult(int custId, int subAccId, IEnumerable<CustBotMessenger> messengers)
        {
            // Assert
            var expected = Result.CreateSuccessfulResult();
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((messengers, Enumerable.Empty<BotFunction>())));

            // Act
            var result = await _settingService.ResendOtp(custId, subAccId);

            // Act
            Assert.True(result.Code == expected.Code && result.Message == expected.Message);
        }

        [Theory, AutoData]
        public async Task Link_CouldNotFoundMessenger_ReturnsMessengerNotFoundResult(int botId, int custId, int subAccId, string otp, string username, string ip)
        {
            // Assert
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((Enumerable.Empty<CustBotMessenger>(), Enumerable.Empty<BotFunction>())));

            // Act
            var result = await _settingService.Link(botId, custId, subAccId, otp, username, ip);

            // Act
            Assert.True(result.Code == ErrorCodes.MessengerNotFound && result.Message == "Messenger not found");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Link_ExpectedOtpIsNullOrEmpty_ReturnsOTPExpiredResult(string expectedOtp)
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _cacheService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult(expectedOtp));

            // Act
            var result = await _settingService.Link(fixture.Create<int>(), fixture.Create<int>(), fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());

            // Act
            Assert.True(result.Code == ErrorCodes.OTP_Expired && result.Message == "OTP is expired");
        }

        [Theory, AutoData]
        public async Task Link_InvalidOtp_ReturnsOTPInvalidResult(int botId, int custId, int subAccId, string username, string ip)
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            var otp = "123";
            var expectedOtp = "456";
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _cacheService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult(expectedOtp));

            // Act
            var result = await _settingService.Link(botId, custId, subAccId, otp, username, ip);

            // Act
            Assert.True(result.Code == ErrorCodes.OTP_Invalid && result.Message == "OTP is invalid");
        }

        [Theory, AutoData]
        public async Task Link_OTPFailedManyTimes_ReturnsOTPExpiredResult(int botId, int custId, int subAccId, string username, string ip)
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            var otp = "123";
            var expectedOtp = "456";
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _cacheService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult(expectedOtp));
            _cacheService.GetAsync<int>(Arg.Any<string>()).Returns(Task.FromResult(2));

            // Act
            var result = await _settingService.Link(botId, custId, subAccId, otp, username, ip);

            // Act
            Assert.True(result.Code == ErrorCodes.OTP_FailedManyTimes && result.Message == "OTP failed many times");
        }

        [Theory, AutoData]
        public async Task Link_ValidOTP_CallsUpdateAccountStatus(int botId, int custId, int subAccId, string username, string ip, CustomerStatus status)
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            var otp = "123";
            var expectedOtp = "123";
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _cacheService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult(expectedOtp));
            _cacheService.GetAsync<int>(Arg.Any<string>()).Returns(Task.FromResult(0));
            _customerService.GetCustomerStatus(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(status));
            _dynamicRepository.FetchMultipleAsync<CustBotAccount, BotFunction>(Arg.Any<CustBotUserSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotAccount>>(), Enumerable.Empty<BotFunction>())));

            // Act
            await _settingService.Link(botId, custId, subAccId, otp, username, ip);

            // Act
            await _dynamicRepository.Received().ExecuteAsync(Arg.Is<CustBotAccountStatusUpdCriteria>(c =>
                c.BotId == botId && c.CustId == custId && c.SubAccId == subAccId && c.Status && c.IsVerifyOTP && c.IP == ip));
        }

        [Theory, AutoData]
        public async Task Unlink_CouldNotFoundMessenger_ReturnsMessengerNotFoundResult(int custId, int subAccId, string ip)
        {
            // Assert
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((Enumerable.Empty<CustBotMessenger>(), Enumerable.Empty<BotFunction>())));

            // Act
            var result = await _settingService.Unlink(custId, subAccId, ip);

            // Act
            Assert.True(result.Code == ErrorCodes.MessengerNotFound && result.Message == "Messenger not found");
        }

        [Theory, AutoData]
        public async Task Unlink_MessengerFounded_CallsUpdateAccountStatus(int custId, int subAccId, string ip, IEnumerable<CustBotMessenger> messengers, CustomerStatus status, IEnumerable<CustBotAccount> accounts)
        {
            // Assert
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((messengers, Enumerable.Empty<BotFunction>())));
            _customerService.GetCustomerStatus(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(status));
            _dynamicRepository.FetchMultipleAsync<CustBotAccount, BotFunction>(Arg.Any<CustBotUserSelCriteria>()).Returns(Task.FromResult((accounts, Enumerable.Empty<BotFunction>())));

            // Act
            await _settingService.Unlink(custId, subAccId, ip);

            // Act
            await _dynamicRepository.Received().ExecuteAsync(
                Arg.Is<CustBotAccountStatusUpdCriteria>(c => c.CustId == custId && c.SubAccId == subAccId && !c.Status && !c.IsVerifyOTP && c.IP == ip));
        }
    }
}