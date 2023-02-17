using System;
using WLMonitored.Shared.Criteria;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Criteria;

public class GetCreditAccumulatedWinLossCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetCreditAccumulatedWinLossCriteria("1176070", string.Empty, DateTime.Today);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_AccumulatedDataSel", criteria.GetSettingKey());
    }

    [Fact]
    public void IsValid_Always_ReturnsTrue()
    {
        // Arrange
        var criteria = new GetCreditAccumulatedWinLossCriteria(string.Empty, string.Empty, DateTime.Today);

        // Act & Assert
        Assert.True(criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        var now = DateTime.Today;
        var srecommends = "1176086";
        var criteria = new GetCreditAccumulatedWinLossCriteria("1176070", string.Empty, now, srecommends);

        // Act & Assert
        Assert.True(
            now == criteria.CurrentDate
            && srecommends == criteria.Recommends);
    }
}