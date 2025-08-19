using WLMonitored.RiskControl.Importer.Criteria;
using Xunit;

namespace WLMonitored.RiskControl.Importer.UnitTests.Criteria;

public class GetWinLossSettingCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetWinLossSettingCriteria("23003140", string.Empty, false);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_GetSettingByCust", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("     ", false)]
    [InlineData("23003140", true)]
    public void IsValid_CustList_ReturnsCorrectExpected(string custList, bool expected)
    {
        // Arrange
        var criteria = new GetWinLossSettingCriteria(custList, string.Empty, false);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Fact]
    public void Constructor_Always_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var criteria = new GetWinLossSettingCriteria("23003140", "1,2,3,4,5,6", false);

        // Assert
        Assert.True(
            0 == criteria.SportType
            && !criteria.IsDeposit
            && "1,2,3,4,5,6" == criteria.SuperList
            && !criteria.IsTest
        );
    }
}