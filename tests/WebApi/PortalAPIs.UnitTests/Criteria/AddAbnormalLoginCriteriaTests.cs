using Xunit;
using PortalAPIs.Criteria;

namespace PortalAPIs.UnitTests.Criteria;

public class AddAbnormalLoginCriteriaTests
{
    [Fact]
    public void Constructor_WithValidBatch_SetsNewBatch()
    {
        // Arrange
        string expectedBatch = "test-batch";

        // Act
        var criteria = new AddAbnormalLoginCriteria(expectedBatch);

        // Assert
        Assert.Equal(expectedBatch, criteria.NewBatch);
    }

    [Fact]
    public void IsValid_WithValidBatch_ReturnsTrue()
    {
        // Arrange
        var criteria = new AddAbnormalLoginCriteria("valid-batch");

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithInvalidBatch_ReturnsFalse(string? invalidBatch)
    {
        // Arrange
        var criteria = new AddAbnormalLoginCriteria(invalidBatch);

        // Act
        var result = criteria.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetSettingKey_ReturnsCorrectKey()
    {
        // Arrange
        var criteria = new AddAbnormalLoginCriteria("test-batch");

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Acc_CustAbnormalLoginIns", result);
    }
}