using FluentAssertions;
using MemberAPIs.Criteria;
using Xunit;

namespace MemberAPIs.UnitTests.Criteria;

public class AssignPersonalMessageCriteriaTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        long messageId = 12345;
        int custId = 67890;
        string custName = "John Doe";

        // Act
        var criteria = new AssignPersonalMessageCriteria(messageId, custId, custName);

        // Assert
        criteria.MessageId.Should().Be(messageId);
        criteria.CustId.Should().Be(custId);
        criteria.CustName.Should().Be(custName);
        criteria.BetTransId.Should().Be("0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsValid_WithInvalidMessageId_ReturnsFalse(long invalidMessageId)
    {
        // Arrange
        var criteria = new AssignPersonalMessageCriteria(invalidMessageId, 67890, "John Doe");

        // Act
        bool result = criteria.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void IsValid_WithValidMessageId_ReturnsTrue(long validMessageId)
    {
        // Arrange
        var criteria = new AssignPersonalMessageCriteria(validMessageId, 67890, "John Doe");

        // Act
        bool result = criteria.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectValue()
    {
        // Arrange
        var criteria = new AssignPersonalMessageCriteria(12345, 67890, "John Doe");

        // Act
        string settingKey = criteria.GetSettingKey();

        // Assert
        settingKey.Should().Be("Age_Msg_SetUsrMessage");
    }
}