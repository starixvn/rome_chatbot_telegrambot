using TelegramBot.Importer.Criteria.Customers;

namespace TelegramBot.Importer.UnitTests.Criteria.Customers;

public class GetYesterdayWinLossLinkedCustomerCommandTests
{
    [Theory, AutoData]
    public void GetSettingKey_Always_ReturnsCorrectKey(DateTime winLostDate, string serviceName, string environment)
    {
        // Arrange
        var criteria = new GetYesterdayWinLossLinkedCustomerCommand(winLostDate, serviceName, environment);

        // Act
        var actualResult = criteria.GetSettingKey();

        // Assert
        Assert.Equal("Age_Chatbot_YesterdateWinlost_LinkedAccounts_Sel", actualResult);
    }

    [Theory, AutoData]
    public void IsValid_Always_ReturnsTrue(DateTime winLostDate, string serviceName, string environment)
    {
        // Arrange
        var criteria = new GetYesterdayWinLossLinkedCustomerCommand(winLostDate, serviceName, environment);

        // Act
        var actualResult = criteria.IsValid();

        // Assert
        Assert.True(actualResult);
    }
}