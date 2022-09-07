using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.UnitTests.Models;

public class CustomerStatusTests
{
    [Fact]
    public void GetStatus_DisabledOrCloseAccount_ReturnsCorrectValue()
    {
        // Arrange
        var expectedValue = "Customer Status: Disabled or Closed";

        // Act
        var status = new CustomerStatus(1, 2, false, false, false, string.Empty);

        // Assert
        Assert.Equal(expectedValue, status.GetStatus("[A]"));
    }

    [Fact]
    public void GetStatus_IsProtectedMode_ReturnsCorrectValue()
    {
        // Arrange
        var expectedValue = "Customer Status: Protected";

        // Act
        var status = new CustomerStatus(1, 2, true, true, false, string.Empty);

        // Assert
        Assert.Equal(expectedValue, status.GetStatus("[A]"));
    }

    [Fact]
    public void GetStatus_SubAccountDoesNOTHasPermission_ReturnsCorrectValue()
    {
        // Arrange
        var expectedValue = "Customer Status: Sub-Account doesn't has [H] permission";

        // Act
        var status = new CustomerStatus(1, 2, true, false, false, "[A],[B]");

        // Assert
        Assert.Equal(expectedValue, status.GetStatus("[H]"));
    }

    [Fact]
    public void GetStatus_AccountIsLocked_ReturnsCorrectValue()
    {
        // Arrange
        var expectedValue = "Customer Status: Locked";

        // Act
        var status = new CustomerStatus(1, 0, true, false, true, string.Empty);

        // Assert
        Assert.Equal(expectedValue, status.GetStatus("[H]"));
    }

    [Fact]
    public void IsInvalid_ValidAccount_ReturnsFalse()
    {
        // Act
        var status = new CustomerStatus(1, 0, true, false, false, string.Empty);

        // Assert
        Assert.False(status.IsInvalid("[H]", out _));
    }

    [Fact]
    public void IsInvalid_InvalidAccount_ReturnsTrue()
    {
        // Act
        var status = new CustomerStatus(1, 0, false, false, false, string.Empty);

        // Assert
        Assert.True(status.IsInvalid("[H]", out _));
    }
}