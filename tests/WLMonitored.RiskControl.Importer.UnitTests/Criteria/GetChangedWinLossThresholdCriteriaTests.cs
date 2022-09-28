using System;
using WLMonitored.RiskControl.Importer.Criteria;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Criteria;

public class GetChangedWinLossThresholdCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetChangedWinLossThresholdCriteria(string.Empty, false, DateTime.Now);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_GetChangedSettingList", criteria.GetSettingKey());
    }
    
    [Fact]
    public void IsValid_Always_ReturnsTrue()
    {
        // Arrange
        var criteria = new GetChangedWinLossThresholdCriteria(string.Empty, false, DateTime.Now);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var criteria = new GetChangedWinLossThresholdCriteria("1,2,3,4,5,6", false, DateTime.Now);

        // Assert
        Assert.True(
            0 == criteria.SportType
            && !criteria.IsDeposit
            && "1,2,3,4,5,6" == criteria.SuperList
            && !criteria.IsTest
        );
    }
}