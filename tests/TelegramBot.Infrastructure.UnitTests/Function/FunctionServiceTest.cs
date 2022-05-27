using System.Collections.Generic;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.UnitTests.Function;

public class FunctionServiceTest
{
    private IEnumerable<BotFunction> _functions;

    private readonly IDynamicRepository dynamicRepository;
    private readonly WebAPIConfiguration webAPIConfiguration;
    private readonly ResourceService resourceService;
    private FunctionService functionService;

    public FunctionServiceTest()
    {
        webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
        dynamicRepository = Substitute.For<IDynamicRepository>();
        resourceService = Substitute.For<ResourceService>();
        functionService = new FunctionService(dynamicRepository, webAPIConfiguration, resourceService);
    }
}