

namespace TelegramBot.Infrastructure.UnitTests.Function
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Data.Repository;
    using Newtonsoft.Json;
    using NSubstitute;
    using TelegramBot.Infrastructure.Function.Models;
    using TelegramBot.Infrastructure.Setting;
    using TelegramBot.Infrastructure.Shared.Configurations;
    using Xunit;
    using TelegramBot.Infrastructure.Function.Constants;
    using TelegramBot.Infrastructure.Resource;
    using TelegramBot.Infrastructure.Resource.Models;
    using Fanex.Caching;
    using TelegramBot.Infrastructure.Shared.Caching;
    using AutoFixture.Xunit2;
    using System;

    public class FunctionServiceTest
    {
        private readonly IDynamicRepository _dynamicRepository;
        private readonly FunctionService _funcService;
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IResourceService _resourceService;
        private readonly ICacheService cacheService;

        public FunctionServiceTest()
        {
            _dynamicRepository = Substitute.For<IDynamicRepository>();
            _webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            _resourceService = Substitute.For<IResourceService>();
            cacheService = Substitute.For<ICacheService>();
            var cacheServiceFactory = Substitute.For<ICacheServiceFactory>();
            cacheServiceFactory.Get(Arg.Any<string>()).Returns(cacheService);
            _funcService = new FunctionService(_dynamicRepository, _webAPIConfiguration, _resourceService, cacheServiceFactory);
        }

        [Fact]
        public async Task GetFunctionName_Always_ReturnName()
        {
            // Arrange
            IEnumerable<BotFunction> botEntities = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a" ,Command = "m"},
                new() { FunctionId = 1, FunctionName = "a", Type =FunctionTypes.Common}
            };
            _dynamicRepository.FetchAsync<BotFunction>(Arg.Any<ICriteria>()).Returns(botEntities);
            _resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new BotResource("cmd", "cmd", "en", "functionsName"));
            // Act
            var result = await _funcService.GetFunctionName(1, "EN").ConfigureAwait(false);

            // Assert
            const string expectedResult = "functionsName";
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task GetFunctionDescription_Always_ReturnDescription()
        {
            // Arrange
            IEnumerable<BotFunction> botEntities = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a" ,Command = "m", Description = "AAAA"},
                new() { FunctionId = 1, FunctionName = "a", Type =FunctionTypes.Common}
            };
            _dynamicRepository.FetchAsync<BotFunction>(Arg.Any<ICriteria>()).Returns(botEntities);
            _resourceService.GetResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new BotResource("cmd", "cmd", "en", "Description"));
            // Act
            var result = await _funcService.GetFunctionDescription(1, "EN").ConfigureAwait(false);

            // Assert
            const string expectedResult = "Description";
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }

        [Theory, AutoData]
        public async Task GetAll_Always_ReturnAllFunctions(IEnumerable<BotFunction> functions)
        {
            // Arrange
            cacheService.GetOrSetAsync(
                        Arg.Any<string>(),
                        Arg.Any<Func<Task<IEnumerable<BotFunction>>>>(),
                        Arg.Any<CacheItemOptions>())
                        .Returns(Task.FromResult(functions));

            // Act
            var result = await _funcService.GetAll();

            //Assert
            Assert.Same(functions, result);
        }

        [Theory, AutoData]
        public async Task GetReportAndCommonFunctions_Always_CallsGetFunctionFromCache(IEnumerable<BotFunction> functions)
        {
            // Arrange
            cacheService.GetOrSetAsync(
                        Arg.Any<string>(),
                        Arg.Any<Func<Task<IEnumerable<BotFunction>>>>(),
                        Arg.Any<CacheItemOptions>())
                        .Returns(Task.FromResult(functions));

            // Act
            await _funcService.GetReportAndCommonFunctions();

            //Assert
            await cacheService.Received().GetOrSetAsync(
                "TelegramBot.Infrastructure.Function.FunctionService.AllFunctions",
                Arg.Any<Func<Task<IEnumerable<BotFunction>>>>(),
                Arg.Any<CacheItemOptions>());
        }
    }
}