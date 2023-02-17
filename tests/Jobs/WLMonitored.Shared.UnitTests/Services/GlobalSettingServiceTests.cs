using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using NSubstitute;
using WLMonitored.Shared.Criteria;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Services;

public class GlobalSettingServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly IGlobalSettingService globalSettingService;

    public GlobalSettingServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        globalSettingService = new GlobalSettingService(dynamicRepository);
    }

    [Fact]
    public async Task GetGlobalSetting_Empty_ThrowsKeyNotFoundException()
    {
        // Arrange
        dynamicRepository
            .FetchAsync<GlobalSettingEntity>(Arg.Any<GetGlobalSettingCriteria>())
            .Returns(Task.FromResult(Enumerable.Empty<GlobalSettingEntity>()));
        
        // Act
        var actual = await globalSettingService.GetGlobalSetting(string.Empty);
        
        // Assert
        Assert.Throws<KeyNotFoundException>(() => actual.LastId);
    }

    [Fact]
    public async Task GetGlobalSetting_HasData_ReturnsCorrectGlobalSetting()
    {
        // Arrange
        const string jobName = "AccumulateWinLossJob";
        var settings = new List<GlobalSettingEntity>
        {
            new GlobalSettingEntity("UAT-WLMonitoredAccount", jobName, "0")
        };
        dynamicRepository
            .FetchAsync<GlobalSettingEntity>(Arg.Any<GetGlobalSettingCriteria>())
            .Returns(Task.FromResult<IEnumerable<GlobalSettingEntity>>(settings));
        
        // Act
        var actual = await globalSettingService.GetGlobalSetting("UAT");
        
        // Assert
        Assert.True(actual.IsStopJob(jobName));
    }
}