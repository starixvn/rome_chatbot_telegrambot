using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using NSubstitute;
using WLMonitored.RiskControl.Importer.Criteria;
using WLMonitored.RiskControl.Importer.Models;
using WLMonitored.RiskControl.Importer.Services;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Services;

public class PlayerHistoryServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly IPlayerHistoryService playerHistoryService;

    public PlayerHistoryServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        playerHistoryService = new PlayerHistoryService(dynamicRepository);
    }

    [Fact]
    public async Task Insert_Always_CallsExecuteAsync()
    {
        // Arrange
        var reachPlayerHistoryParams = new List<ReachPlayerHistoryParam>
        {
            new(23003140, 100, 0, 1, 200, 300, 2)
        };
        byte notificationType = 1;
        var expected = "<CustIds><Cust CustId=\"23003140\" AccumWL=\"100\" SettingType=\"1\" Winningthreshold=\"200\" Losingthreshold=\"300\" ReachType=\"2\"/></CustIds>";

        // Act
        await playerHistoryService.Insert(reachPlayerHistoryParams, notificationType, null, null, "WLMonitoredAccount");

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ReachPlayerHistoryCommand>(
            criteria => expected == criteria.CustReached
            && notificationType == criteria.NotificationType
            && null == criteria.LastCheckTime
            && null == criteria.LastScanSettingDate
        ));
    }
}