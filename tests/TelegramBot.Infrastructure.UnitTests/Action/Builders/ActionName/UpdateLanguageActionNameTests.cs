using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using TelegramBot.Infrastructure.Action.Builders.ActionName;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.Models;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action.Builders.ActionName;

public class UpdateLanguageActionNameTests
{
    private readonly IResourceService resourceService;
    private readonly UpdateLanguageActionName updateLanguageActionName;

    public UpdateLanguageActionNameTests()
    {
        resourceService = Substitute.For<IResourceService>();
        updateLanguageActionName = new UpdateLanguageActionName(resourceService);
    }

    [Theory, AutoData]
    public async Task BuildActionName_IsCurrentLanguage_ReturnsCorrectNameWithGreenTick(CustBotAccount account)
    {
        // Arrange
        account.Language = LanguageCodes.English;
        resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new BotResource("Action", ActionIds.English.ToString(), LanguageCodes.English, nameof(LanguageCodes.English))));
        var botAction = new BotAction() { ActionId = ActionIds.English };

        // Act
        var result = await updateLanguageActionName.BuildActionName(botAction, account);

        // Assert
        Assert.Equal("\u2705 English \uD83C\uDDEC\uD83C\uDDE7", result);
    }

    [Theory]
    [InlineData(ActionIds.English, LanguageCodes.English, Unicode.UKFlag)]
    [InlineData(ActionIds.ChineseTraditional, LanguageCodes.ChineseTraditional, Unicode.ChinaFlag)]
    [InlineData(ActionIds.ChineseSimplified, LanguageCodes.ChineseSimplified, Unicode.ChinaFlag)]
    [InlineData(ActionIds.Thai, LanguageCodes.Thai, Unicode.ThaiFlag)]
    [InlineData(ActionIds.Vietnamese, LanguageCodes.Vietnamese, Unicode.VietnamFlag)]
    public async Task BuildActionName_ValidActionId_ReturnsCorrectName(short actionId, string languageCode, string flag)
    {
        // Arrange
        var account = new Fixture().Create<CustBotAccount>();
        resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new BotResource("Action", actionId.ToString(), languageCode, "Language")));
        var botAction = new BotAction() { ActionId = actionId };

        // Act
        var result = await updateLanguageActionName.BuildActionName(botAction, account);

        // Assert
        Assert.Contains($"Language {flag}", result);
    }
}