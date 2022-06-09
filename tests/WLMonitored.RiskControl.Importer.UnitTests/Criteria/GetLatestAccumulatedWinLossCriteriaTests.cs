using System;
using WLMonitored.RiskControl.Importer.Criteria;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Criteria;

public class GetLatestAccumulatedWinLossCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetLatestAccumulatedWinLossCriteria(DateTime.Now, 1000);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_LatestAccumulatedDataSel", criteria.GetSettingKey());
    }
    
    [Theory]
    [InlineData(-100, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsValid_BatchNo_ReturnsCorrectExpected(int batchNo, bool expected)
    {
        // Arrange
        var criteria = new GetLatestAccumulatedWinLossCriteria(DateTime.Now, batchNo);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var criteria = new GetLatestAccumulatedWinLossCriteria(now, 1000);

        // Assert
        Assert.Equal(now, criteria.LastCheckTime);
    }
}