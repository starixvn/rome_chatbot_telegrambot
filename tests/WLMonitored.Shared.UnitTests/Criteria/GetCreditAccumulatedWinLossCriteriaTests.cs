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
        var criteria = new GetCreditAccumulatedWinLossCriteria("1176070", DateTime.Today);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_AccumulatedDataSel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("     ", false)]
    [InlineData("1176070", true)]
    public void IsValid_CustIds_ReturnsExpected(string custIds, bool expected)
    {
        // Arrange
        var criteria = new GetCreditAccumulatedWinLossCriteria(custIds, DateTime.Today);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange
        var now = DateTime.Today;
        var srecommends = "1176086";
        var criteria = new GetCreditAccumulatedWinLossCriteria("1176070", now, srecommends);

        // Act & Assert
        Assert.True(
            now == criteria.CurrentDate
            && srecommends == criteria.Recommends);
    }
}