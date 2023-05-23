using WLMonitored.AccumWLService.Criteria;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Criteria;

public class UpdateAccumulateWinLossCommandTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new UpdateAccumulateWinLossCommand("1176070", 101, "WLMonitoredAccount");

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_AccumulatedData", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("     ", false)]
    [InlineData("1176070", true)]
    public void IsValid_CustWinlost_ReturnsExpectedResult(string custWinlost, bool expected)
    {
        // Arrange
        var criteria = new UpdateAccumulateWinLossCommand(custWinlost, 1000, "WLMonitoredAccount");

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Theory]
    [InlineData(-11176070, false)]
    [InlineData(0, false)]
    [InlineData(200, true)]
    public void IsValid_LastId_ReturnsExpectedResult(long lastId, bool expected)
    {
        // Arrange
        var criteria = new UpdateAccumulateWinLossCommand("1176070", lastId, "WLMonitoredAccount");

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }
}