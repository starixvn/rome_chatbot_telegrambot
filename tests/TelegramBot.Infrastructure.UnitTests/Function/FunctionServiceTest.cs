using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Resource.Models;

namespace TelegramBot.Infrastructure.UnitTests.Function
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Data.Repository;
    using Newtonsoft.Json;
    using NSubstitute;
    using TelegramBot.Infrastructure.Function.Models;
    using Resource;
    using TelegramBot.Infrastructure.Setting;
    using Shared.Configurations;
    using Xunit;

    public class FunctionServiceTest
    {
        private readonly IDynamicRepository _dynamicRepository;
        private readonly FunctionService _funcService;
        private readonly WebAPIConfiguration _webAPIConfiguration;
        private readonly IResourceService _resourceService;

        public FunctionServiceTest()
        {
            _dynamicRepository = Substitute.For<IDynamicRepository>();
            _webAPIConfiguration = Substitute.For<WebAPIConfiguration>();
            _resourceService = Substitute.For<IResourceService>();
            _funcService = new FunctionService(_dynamicRepository, _webAPIConfiguration,_resourceService);
        }

        [Fact]
        public async Task GetCommonFunctions_Always_ReturnCommonFunctions()
        {
            // Arrange
            IEnumerable<BotFunction> botEntities = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a" },
                new() { FunctionId = 1, FunctionName = "a", Type =FunctionTypes.Common}
            };
            _dynamicRepository.FetchAsync<BotFunction>(Arg.Any<ICriteria>()).Returns(botEntities);
            // Act
            var result = await _funcService.GetCommonFunctions();

            // Assert
            var expectedResult = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a", Type =FunctionTypes.Common}
            };
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }    
        
        [Fact]
        public async Task GetReportFunctions_Always_ReturnReportFunctions()
        {
            // Arrange
            IEnumerable<BotFunction> botEntities = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a" },
                new() { FunctionId = 1, FunctionName = "a", Type =FunctionTypes.Report}
            };
            _dynamicRepository.FetchAsync<BotFunction>(Arg.Any<ICriteria>()).Returns(botEntities);
            // Act
            var result = await _funcService.GetReportFunctions();

            // Assert
            var expectedResult = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a", Type = FunctionTypes.Report}
            };
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        } 
        
        [Fact]
        public async Task GetFunctionByCommand_Always_ReturnFunction()
        {
            // Arrange
            IEnumerable<BotFunction> botEntities = new List<BotFunction>
            {
                new() { FunctionId = 1, FunctionName = "a" ,Command = "m"},
                new() { FunctionId = 1, FunctionName = "a", Type =FunctionTypes.Common}
            };
            _dynamicRepository.FetchAsync<BotFunction>(Arg.Any<ICriteria>()).Returns(botEntities);
            // Act
            var result = await _funcService.GetFunctionByCommand("m");

            // Assert
            var expectedResult = new BotFunction { FunctionId = 1, FunctionName = "a" ,Command = "m"};
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
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
                .Returns(new BotResource("cmd","cmd","en","functionsName"));
            // Act
            var result = await _funcService.GetFunctionName(1,"EN").ConfigureAwait(false);

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
                .Returns(new BotResource("cmd","cmd","en","Description"));
            // Act
            var result = await _funcService.GetFunctionDescription(1,"EN").ConfigureAwait(false);

            // Assert
            const string expectedResult = "Description";
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }
    }
}