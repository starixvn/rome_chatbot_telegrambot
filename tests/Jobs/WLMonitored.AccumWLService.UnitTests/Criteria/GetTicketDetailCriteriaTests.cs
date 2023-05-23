using WLMonitored.AccumWLService.Criteria;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Criteria;

public class GetTicketDetailCriteriaTests
{
    [Fact]
    public void GetSettingKey_Always_ReturnsValidSettingKey()
    {
        // Arrange
        var criteria = new GetTicketDetailCriteria(1, 1000);

        // Act & Assert
        Assert.Equal("Acc_WLMonitored_Service_TicketDetailSel", criteria.GetSettingKey());
    }

    [Theory]
    [InlineData(-1176070, false)]
    [InlineData(0, true)]
    [InlineData(10, true)]
    public void IsValid_LastId_ReturnsExpectedResult(long lastId, bool expected)
    {
        // Arrange
        var criteria = new GetTicketDetailCriteria(lastId, 1000);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }

    [Theory]
    [InlineData(-1176070, false)]
    [InlineData(0, false)]
    [InlineData(1000, true)]
    public void IsValid_BatchNo_ReturnsExpectedResult(int batchNo, bool expected)
    {
        // Arrange
        var criteria = new GetTicketDetailCriteria(1, batchNo);

        // Act & Assert
        Assert.Equal(expected, criteria.IsValid());
    }
}