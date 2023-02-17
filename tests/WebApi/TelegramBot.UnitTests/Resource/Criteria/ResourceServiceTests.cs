using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Fanex.Caching;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Modules.Resource;
using TelegramBot.Modules.Resource.constants;
using TelegramBot.Modules.Resource.Models;
using TelegramBot.Shared.Caching;
using TelegramBot.Shared.Constants;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Resource;

public class ResourceServiceTests
{
    private readonly ICacheService cacheService;
    private readonly IDynamicRepository dynamicRepository;
    private readonly ICacheServiceFactory cacheServiceFactory;
    private readonly ResourceService resourceService;

    public ResourceServiceTests()
    {
        cacheService = Substitute.For<ICacheService>();
        dynamicRepository = Substitute.For<IDynamicRepository>();
        cacheServiceFactory = Substitute.For<ICacheServiceFactory>();
        cacheServiceFactory.Get().Returns(cacheService);
        resourceService = new ResourceService(cacheServiceFactory, dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetResource_Always_ReturnsCorrectValue(string key, string type)
    {
        // Arrange
        IEnumerable<BotResource> botResources = new List<BotResource>() { new BotResource(type, key, LanguageCodes.English, "value") };
        cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<IEnumerable<BotResource>>>>(),
            Arg.Any<CacheItemOptions>())
            .Returns(Task.FromResult(botResources));

        // Act
        var result = await resourceService.GetResource(key, type, LanguageCodes.English);

        // Assert
        Assert.Same(botResources.FirstOrDefault(), result);
    }

    [Theory, AutoData]
    public async Task GetActionDescription_Always_RetrunsCorrectValue(short actionId, string language)
    {
        // Arrange
        var actionDescription = "actionDescription";
        IEnumerable<BotResource> botResources = new List<BotResource>() { new BotResource(ResourceType.ActionDescription, actionId.ToString(), language, actionDescription) };
        cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<IEnumerable<BotResource>>>>(),
            Arg.Any<CacheItemOptions>())
            .Returns(Task.FromResult(botResources));

        // Act
        var result = await resourceService.GetActionDescription(actionId, language);

        // Assert
        Assert.Equal(result, actionDescription);
    }
}