using TelegramBot.Importer.Criteria;

namespace TelegramBot.Importer.UnitTests.Criteria;

public class GetOfferedProductCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(int custId)
    {
        // Arrange
        var criteria = new GetOfferedProductCriteria(custId);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_GetOfferedProductByCust", actualResult);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void IsValid_InvalidCustId_ReturnsFalse(int custId)
    {
        // Arrange
        var criteria = new GetOfferedProductCriteria(custId);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory]
    [InlineData(1)]
    public void IsValid_ValidCustId_ReturnsTrue(int custId)
    {
        // Arrange
        var criteria = new GetOfferedProductCriteria(custId);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}