using WLMonitored.AccumWLService.Criteria;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Criteria;

public class GetWinLossSettingCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetWinLossSettingCriteria("1176070", string.Empty, false);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_CheckExistSettingByCust", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("     ", false)]
    [InlineData("1176070", true)]
    public void IsValid_CustList_ReturnsExpectedResult(string custList, bool expected)
    {
        // Arrange
        var criteria = new GetWinLossSettingCriteria(custList, string.Empty, false);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsDefaultValues()
    {
        // Arrange
        var criteria = new GetWinLossSettingCriteria("1176070", "1,2,3,4,5,6", false);

        // Act & Assert
        Assert.True(
            0 == criteria.SportType
            && "1,2,3,4,5,6" == criteria.SuperList
            && !criteria.IsTest
        );
    }
}