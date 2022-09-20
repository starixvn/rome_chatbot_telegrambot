using TelegramBot.Importer.Criteria.Customers;

namespace TelegramBot.Importer.UnitTests.Criteria.Customers;

public class GetLinkedCustomerByFunctionCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string functionKey, string environment)
    {
        // Arrange
        var criteria = new GetLinkedCustomerByFunctionCriteria(functionKey, environment);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_GetAllLinkedAccountsByFunction", actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_FunctionKeyIsNullOrEmpty_ReturnsFalse(string functionKey)
    {
        // Arrange
        var criteria = new GetLinkedCustomerByFunctionCriteria(functionKey, "environment");

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory, AutoData]
    public void IsValid_ValidCriteria_ReturnsTrue(string functionKey, string environment)
    {
        // Arrange
        var criteria = new GetLinkedCustomerByFunctionCriteria(functionKey, environment);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}