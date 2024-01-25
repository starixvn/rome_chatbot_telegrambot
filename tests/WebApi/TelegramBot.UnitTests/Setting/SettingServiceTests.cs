using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Services;
using AutoFixture;
using AutoFixture.Xunit2;
using Fanex.Data.Repository;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Modules.Action;
using TelegramBot.Modules.AutoTransfer;
using TelegramBot.Modules.Function.Models;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.OTP;
using TelegramBot.Modules.Setting;
using TelegramBot.Modules.Setting.Constants;
using TelegramBot.Modules.Setting.Criteria;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Caching;
using TelegramBot.Shared.Configurations;
using TelegramBot.Shared.Models;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Setting
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
        private readonly IOTPService _otpService;
        private readonly ICacheServiceFactory _cacheServiceFactory;
        private readonly IAutoTransferService _autoTransferService;

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
            _otpService = Substitute.For<IOTPService>();
            _cacheServiceFactory = Substitute.For<ICacheServiceFactory>();
            _autoTransferService = Substitute.For<IAutoTransferService>();
            _settingService = new SettingService(
                _webAPIConfiguration,
                _dynamicRepository,
                _messageSender,
                _messageBuilder,
                _actionService,
                _customerService,
                _functionService,
                _otpService,
                _autoTransferService,
                _cacheServiceFactory);
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

        [Fact]
        public async Task Link_ExpectedOtpIsNullOrEmpty_ReturnsOTPExpiredResult()
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _otpService.VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(new Result(ErrorCodes.OTP_Expired, "OTP is expired")));
            // Act
            var result = await _settingService.Link(fixture.Create<int>(), fixture.Create<int>(), fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());

            // Act
            Assert.True(result.Code == ErrorCodes.OTP_Expired && result.Message == "OTP is expired");
        }

        [Fact]
        public async Task Link_InvalidOtp_ReturnsOTPInvalidResult()
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _otpService.VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(new Result(ErrorCodes.OTP_Invalid, "OTP is invalid")));

            // Act
            var result = await _settingService.Link(fixture.Create<int>(), fixture.Create<int>(), fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());

            // Act
            Assert.True(result.Code == ErrorCodes.OTP_Invalid && result.Message == "OTP is invalid");
        }

        [Fact]
        public async Task Link_OTPFailedManyTimes_ReturnsOTPExpiredResult()
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _otpService.VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(new Result(ErrorCodes.OTP_FailedManyTimes, "OTP failed many times")));

            // Act
            var result = await _settingService.Link(fixture.Create<int>(), fixture.Create<int>(), fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());

            // Act
            Assert.True(result.Code == ErrorCodes.OTP_FailedManyTimes && result.Message == "OTP failed many times");
        }

        [Theory, AutoData]
        public async Task Link_ValidOTP_CallsUpdateAccountStatus(int botId, int custId, int subAccId, string username, string ip, CustomerStatus status)
        {
            // Assert
            var fixture = new Fixture();
            _webAPIConfiguration.OtpMaxFailedTimes = 3;
            _dynamicRepository.FetchMultipleAsync<CustBotMessenger, BotFunction>(Arg.Any<CustBotMessengerSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotMessenger>>(), Enumerable.Empty<BotFunction>())));
            _customerService.GetCustomerStatus(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(status));
            _dynamicRepository.FetchMultipleAsync<CustBotAccount, BotFunction>(Arg.Any<CustBotUserSelCriteria>()).Returns(Task.FromResult((fixture.Create<IEnumerable<CustBotAccount>>(), Enumerable.Empty<BotFunction>())));
            _otpService.VerifyOTP(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(Result.CreateSuccessfulResult()));
            // Act
            await _settingService.Link(botId, custId, subAccId, fixture.Create<string>(), username, ip);

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