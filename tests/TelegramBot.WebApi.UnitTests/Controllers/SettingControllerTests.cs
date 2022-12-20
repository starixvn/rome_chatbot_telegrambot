using AutoFixture.Xunit2;
using NSubstitute;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Command;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared;
using TelegramBot.Infrastructure.Shared.Models;
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
        private readonly IMessageSender _messageSender;
        private readonly ICommandService _commandService;
        private readonly IMessageBuilder _messageBuilder;

        public SettingControllerTests()
        {
            _cryptography = Substitute.For<ICryptography>();
            _settingService = Substitute.For<ISettingService>();
            _botService = Substitute.For<IBotService>();
            _messageSender = Substitute.For<IMessageSender>();
            _commandService = Substitute.For<ICommandService>();
            _messageBuilder = Substitute.For<IMessageBuilder>();
            _settingController = new SettingController(_cryptography, _settingService, _botService, _messageSender, _commandService, _messageBuilder);
        }

        [Theory, AutoData]
        public async Task GenerateStartUriAsync_Always_Calls(GenerateStartUriParam param, BotEntity botEntity)
        {
            // Arrange
            var plainKey = $"{param.CustId}:{param.SubAccId}:{param.Language}";
            var encryptKey = "encryptKey";
            _cryptography.Encrypt(plainKey).Returns(encryptKey);
            _botService.GetAsync(param.System).Returns(Task.FromResult(botEntity));

            // Act
            var result = await _settingController.GenerateStartUri(param).ConfigureAwait(false);

            // Assert
            _cryptography.Received().Encrypt(plainKey);
            await _botService.Received().GetAsync(param.System);
            var expectedResult = $"https://t.me/{botEntity.Username}?start={encryptKey}";
            Assert.Equal(expectedResult, result);
        }

        [Theory, AutoData]
        public async Task LinkAsync_Always_Calls(LinkParam param, Result expectedResult)
        {
            // Arrange
            _settingService.Link(param.BotId, param.CustId, param.SubAccId, param.Otp, param.Username, param.IP)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _settingController.Link(param).ConfigureAwait(false);

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Theory, AutoData]
        public async Task GetCustBotMessengerAsync_Always_Calls(GetCustBotMessengerParam param, CustBotMessenger custBotMessenger)
        {
            // Arrange
            _settingService.GetCustBotMessenger(param.CustId, param.SubAccId, param.Language)
                .Returns(Task.FromResult(custBotMessenger));

            // Act
            var result = await _settingController.GetCustBotMessengerAsync(param).ConfigureAwait(false);

            // Assert
            Assert.Same(custBotMessenger, result);
        }

        [Theory, AutoData]
        public async Task ResendOtp_Always_CallsSettingServiceResendOtp(SendOtpParam param)
        {
            // Act
            await _settingController.ResendOtp(param).ConfigureAwait(false);

            // Assert
            await _settingService.Received().ResendOtp(param.CustId, param.SubAccId);
        }

        [Theory, AutoData]
        public async Task Link_Always_CallsSettingServiceLink(LinkParam param)
        {
            // Act
            await _settingController.Link(param).ConfigureAwait(false);

            // Assert
            await _settingService.Received().Link(param.BotId, param.CustId, param.SubAccId, param.Otp, param.Username, param.IP);
        }

        [Theory, AutoData]
        public async Task Unlink_Always_CallsSettingServiceUnlink(UnlinkParam param)
        {
            // Act
            await _settingController.Unlink(param).ConfigureAwait(false);

            // Assert
            await _settingService.Received().Unlink(param.CustId, param.SubAccId, param.IP);
        }

        [Theory, AutoData]
        public async Task UpdateFunction_Always_CallsSettingServiceUpdateFunction(int userId, UpdateFunctionParam param)
        {
            // Act
            await _settingController.UpdateFunction(userId, param).ConfigureAwait(false);

            // Assert
            await _settingService.Received().UpdateUserFunction(userId, param.BotId, param.Function, param.Creator, param.IP);
        }

        [Theory, AutoData]
        public async Task UpdateLanguage_Always_CallsSettingServiceUpdateLanguage(int userId, UpdateLanguageParam param)
        {
            param.Language = "en-US";

            // Act
            await _settingController.UpdateLanguage(userId, param).ConfigureAwait(false);

            // Assert
            await _settingService.Received().UpdateUserLanguage(userId, param.BotId, param.Language, param.Creator, param.IP);
        }

        [Theory, AutoData]
        public async Task Start_Always_CallsGetBot(StartParam param, BotEntity bot)
        {
            // Arrang
            _botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));

            // Act
            await _settingController.Start(param);

            // Assert
            await _botService.Received().GetAsync(param.MessengerBotId);
        }

        [Theory, AutoData]
        public async Task Start_MessageNotContainTokenAndAccountExisted_SendMainMenuMessage(BotEntity bot, CustBotAccount account)
        {
            // Arrang
            var param = new StartParam() { MessengerBotId = 1, ChatId = 2, Message = "/start" };
            _botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));
            _settingService.GetCustBotAccount(Arg.Any<long>(), Arg.Any<int>()).Returns(Task.FromResult(account));

            // Act
            await _settingController.Start(param);

            // Assert
            await _commandService.Received().SendMainMenuMessage(Arg.Is(account), Arg.Is<Messenger>(m => m.BotId == bot.BotId && m.ChatId == param.ChatId && m.System == bot.System));
        }

        [Theory, AutoData]
        public async Task Start_MessageNotContainTokenAndAccountNOTExisted_SendIntroMessage(BotEntity bot)
        {
            // Arrang
            var param = new StartParam() { MessengerBotId = 1, ChatId = 2, Message = "/start" };
            _botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));
            CustBotAccount account = null;
            _settingService.GetCustBotAccount(Arg.Any<long>(), Arg.Any<int>()).Returns(Task.FromResult(account));
            _messageBuilder.BuildIntroductionMessage(Arg.Any<string>()).Returns("IntroMessage");

            // Act
            await _settingController.Start(param);

            // Assert
            await _messageSender.Received().SendMessage(Arg.Is<Messenger>(m => m.BotId == bot.BotId && m.ChatId == param.ChatId && m.System == bot.System), "IntroMessage");
        }

        [Theory, AutoData]
        public async Task Start_InvalidMessage_SendIntroMessage(BotEntity bot)
        {
            // Arrang
            var param = new StartParam() { MessengerBotId = 1, ChatId = 2, Message = "/start abc xyz" };
            _botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));
            CustBotAccount account = null;
            _settingService.GetCustBotAccount(Arg.Any<long>(), Arg.Any<int>()).Returns(Task.FromResult(account));
            _messageBuilder.BuildIntroductionMessage(Arg.Any<string>()).Returns("IntroMessage");

            // Act
            await _settingController.Start(param);

            // Assert
            await _messageSender.Received().SendMessage(Arg.Is<Messenger>(m => m.BotId == bot.BotId && m.ChatId == param.ChatId && m.System == bot.System), "IntroMessage");
        }

        [Theory, AutoData]
        public async Task Start_InvalidToken_SendIntroMessage(BotEntity bot)
        {
            // Arrang
            var param = new StartParam() { MessengerBotId = 1, ChatId = 2, Message = "/start abc" };
            _botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));
            CustBotAccount account = null;
            _settingService.GetCustBotAccount(Arg.Any<long>(), Arg.Any<int>()).Returns(Task.FromResult(account));
            _messageBuilder.BuildIntroductionMessage(Arg.Any<string>()).Returns("IntroMessage");
            _cryptography.Decrypt(Arg.Any<string>()).Returns("abc");

            // Act
            await _settingController.Start(param);

            // Assert
            await _messageSender.Received().SendMessage(Arg.Is<Messenger>(m => m.BotId == bot.BotId && m.ChatId == param.ChatId && m.System == bot.System), "IntroMessage");
        }

        [Theory, AutoData]
        public async Task Start_ValidToken_CallsSettingServiceStart(BotEntity bot)
        {
            // Arrang
            var param = new StartParam() { MessengerBotId = 1, ChatId = 2, Message = "/start abc", IP = "127.0.0.1" };
            _botService.GetAsync(Arg.Any<long>()).Returns(Task.FromResult(bot));
            CustBotAccount account = null;
            _settingService.GetCustBotAccount(Arg.Any<long>(), Arg.Any<int>()).Returns(Task.FromResult(account));
            _messageBuilder.BuildIntroductionMessage(Arg.Any<string>()).Returns("IntroMessage");
            _cryptography.Decrypt(Arg.Any<string>()).Returns("1:2:en-US");

            // Act
            await _settingController.Start(param);

            // Assert
            await _settingService.Received().Start(bot.BotId, param.ChatId, 1, 2, "en-US", param.IP);
        }
    }
}