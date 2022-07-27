using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Fanex.Caching;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Action.Builders.ActionName;
using TelegramBot.Infrastructure.Action.Criteria;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action;

public class ActionServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly WebAPIConfiguration webAPIConfiguration;
    private readonly IMessageBuilder messageBuilder;
    private readonly IFunctionHandlerFactory functionHandlerFactory;
    private readonly IActionNameBuilderFactory actionNameBuilderFactory;
    private readonly ICacheService cacheService;
    private readonly IResourceService resourceService;
    private readonly ActionService actionService;

    public ActionServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
        messageBuilder = Substitute.For<IMessageBuilder>();
        functionHandlerFactory = Substitute.For<IFunctionHandlerFactory>();
        actionNameBuilderFactory = Substitute.For<IActionNameBuilderFactory>();
        cacheService = Substitute.For<ICacheService>();
        resourceService = Substitute.For<IResourceService>();
        var cacheServiceFactory = Substitute.For<ICacheServiceFactory>();
        cacheServiceFactory.Get(Arg.Any<string>()).Returns(cacheService);
        actionService = new ActionService(dynamicRepository, webAPIConfiguration, messageBuilder, functionHandlerFactory, actionNameBuilderFactory, cacheServiceFactory, resourceService);
    }

    [Theory, AutoData]
    public async Task GetActionDescription_Always_CallsGetResource(short actionId, string language)
    {
        // Act
        await actionService.GetActionDescription(actionId, language);

        // Assert
        await resourceService.Received().GetResource(actionId.ToString(), ResourceType.ActionDescription, language);
    }

    [Theory, AutoData]
    public async Task GetUserActions_Always_CallsFetchAsync(int userId)
    {
        // Act
        await actionService.GetUserActions(userId);

        // Assert
        await dynamicRepository.Received().FetchAsync<CustBotAction>(Arg.Any<CustBotActionInChainSelCriteria>());
    }

    [Theory, AutoData]
    public async Task InsertUserAction_Always_CallsExecuteAsync(int userId, short actionId, string userResponse, string compiledCommand)
    {
        // Act
        await actionService.InsertUserAction(userId, actionId, userResponse, compiledCommand);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotActionInsCriteria>());
    }

    [Theory, AutoData]
    public async Task UpdateUserAction_Always_CallsExecuteAsync(long id, string userResponse, string compiledCommand)
    {
        // Act
        await actionService.UpdateUserAction(id, userResponse, compiledCommand);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotActionCommandUpdCriteria>());
    }

    [Theory, AutoData]
    public async Task ResetUserActions_Always_CallsExecuteAsync(int userId)
    {
        // Act
        await actionService.ResetUserActions(userId);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Any<CustBotActionStatusUpdCriteria>());
    }
}