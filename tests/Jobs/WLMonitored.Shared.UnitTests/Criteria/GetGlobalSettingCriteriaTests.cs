using WLMonitored.Shared.Criteria;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Criteria;

public class GetGlobalSettingCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetGlobalSettingCriteria();

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_GlobalSettingSel", criteria.GetSettingKey());
    }

    [Fact]
    public void IsValid_Always_ReturnsTrue()
    {
        // Arrange
        var criteria = new GetGlobalSettingCriteria();

        // Act & Assert
        Assert.True(criteria.IsValid());
    }
}