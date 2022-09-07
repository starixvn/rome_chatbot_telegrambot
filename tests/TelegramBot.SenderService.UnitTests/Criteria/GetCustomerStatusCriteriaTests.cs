using TelegramBot.SenderService.Criteria;

namespace TelegramBot.SenderService.UnitTests.Criteria;

public class GetCustomerStatusCriteriaTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(string custList)
    {
        // Arrange
        var criteria = new GetCustomerStatusCriteria(custList);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_CustomerStatusSel", actualResult);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_CustListIsNullOrWhiteSpace_ReturnsFalse(string custList)
    {
        // Arrange
        var criteria = new GetCustomerStatusCriteria(custList);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.False(actualResult);
    }

    [Theory, AutoData]
    public void IsValid_CustListIsValid_ReturnsFalse(string custList)
    {
        // Arrange
        var criteria = new GetCustomerStatusCriteria(custList);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}