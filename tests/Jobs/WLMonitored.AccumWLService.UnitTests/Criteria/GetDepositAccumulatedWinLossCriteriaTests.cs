using System;
using WLMonitored.AccumWLService.Criteria;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Criteria;

public class GetDepositAccumulatedWinLossCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetDepositAccumulatedWinLossCriteria("1176070", DateTime.Today);

        // Act & Assert
        Assert.Equal("Acc_Dep_WLMonitored_Service_AccumulatedDataSel", criteria.GetSettingKey());
    }
}