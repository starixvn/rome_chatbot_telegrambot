using TelegramBot.Importer.Criteria.Customers;

namespace TelegramBot.Importer.UnitTests.Criteria.Customers;

public class GetLinkedCustomerCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string functionKey, string custIdList, string environment)
    {
        // Arrange
        var criteria = new GetLinkedCustomerCriteria(functionKey, custIdList, environment, string.Empty);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_GetLinkedAccounts", actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_FunctionKeyIsNullOrWhiteSpace_ReturnsFalse(string functionKey)
    {
        // Arrange
        var criteria = new GetLinkedCustomerCriteria(functionKey, "custIdList", "environment", string.Empty);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_CustIdListIsNullOrWhiteSpace_ReturnsFalse(string custIdList)
    {
        // Arrange
        var criteria = new GetLinkedCustomerCriteria("functionKey", custIdList, "environment", string.Empty);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_SiteListIsNullOrWhiteSpace_ReturnsFalse(string siteList)
    {
        // Arrange
        var criteria = new GetLinkedCustomerCriteria("functionKey", string.Empty, "environment", siteList);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory, AutoData]
    public void IsValid_ValidCriteria_ReturnsTrue(string functionKey, string custIdList, string environment, string sites)
    {
        // Arrange
        var criteria = new GetLinkedCustomerCriteria(functionKey, custIdList, environment, sites);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}