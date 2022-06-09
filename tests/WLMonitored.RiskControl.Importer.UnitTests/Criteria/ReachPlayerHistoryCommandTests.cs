using System;
using WLMonitored.RiskControl.Importer.Criteria;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Criteria;

public class ReachPlayerHistoryCommandTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new ReachPlayerHistoryCommand("<CustIds></CustIds>", 1, null, null, "WLMonitoredAccount");

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_ReachHistoryIns", criteria.GetSettingKey());
    }
    
    [Theory]
    [InlineData("", false)]
    [InlineData("     ", false)]
    [InlineData("<CustIds></CustIds>", true)]
    public void IsValid_CustReached_ReturnsCorrectExpected(string custReached, bool expected)
    {
        // Arrange
        var criteria = new ReachPlayerHistoryCommand(custReached, 2, null, null, "WLMonitoredAccount");

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var now = DateTime.Now;
        var criteria = new ReachPlayerHistoryCommand("<CustIds></CustIds>", 1, now, now, "WLMonitoredAccount");

        // Assert
        Assert.True(
            1 == criteria.NotificationType
            && now == criteria.LastCheckTime
            && now == criteria.LastScanSettingDate
            && "WLMonitoredAccount" == criteria.ServiceName
        );
    }
}