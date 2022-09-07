using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.UnitTests.Models;

public class ResultTests
{
    [Fact]
    public void IsFailed_CodeIsZero_ReturnsFalse()
    {
        // Act
        var result = new Result(0, string.Empty);

        // Assert
        Assert.False(result.IsFailed);
    }

    [Fact]
    public void IsFailed_CodeIsNOTZero_ReturnsTrue()
    {
        // Act
        var result = new Result(1, string.Empty);

        // Assert
        Assert.True(result.IsFailed);
    }
}