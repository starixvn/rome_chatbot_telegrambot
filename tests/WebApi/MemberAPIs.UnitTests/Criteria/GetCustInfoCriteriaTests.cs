using FluentAssertions;
using MemberAPIs.Criteria;
using Xunit;

namespace MemberAPIs.UnitTests.Criteria;

public class GetCustInfoCriteriaTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        int custId = 12345;

        // Act
        var criteria = new GetCustInfoCriteria(custId);

        // Assert
        criteria.CustId.Should().Be(custId);
        criteria.CustName.Should().BeNull();
        criteria.UplineId.Should().Be(0);
        criteria.UplineName.Should().BeNull();
        criteria.FailedCount.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsValid_WithInvalidCustId_ReturnsFalse(int invalidCustId)
    {
        // Arrange
        var criteria = new GetCustInfoCriteria(invalidCustId);

        // Act
        bool result = criteria.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void IsValid_WithValidCustId_ReturnsTrue(int validCustId)
    {
        // Arrange
        var criteria = new GetCustInfoCriteria(validCustId);

        // Act
        bool result = criteria.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectValue()
    {
        // Arrange
        var criteria = new GetCustInfoCriteria(12345);

        // Act
        string settingKey = criteria.GetSettingKey();

        // Assert
        settingKey.Should().Be("API_LoginNotifyInfoSel");
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var criteria = new GetCustInfoCriteria(12345);
        string custName = "John Doe";
        int uplineId = 67890;
        string uplineName = "Jane Smith";
        int failedCount = 3;

        // Act
        criteria.CustName = custName;
        criteria.UplineId = uplineId;
        criteria.UplineName = uplineName;
        criteria.FailedCount = failedCount;

        // Assert
        criteria.CustName.Should().Be(custName);
        criteria.UplineId.Should().Be(uplineId);
        criteria.UplineName.Should().Be(uplineName);
        criteria.FailedCount.Should().Be(failedCount);
    }
}