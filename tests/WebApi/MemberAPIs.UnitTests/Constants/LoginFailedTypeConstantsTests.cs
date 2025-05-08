using MemberAPIs.Constants;
using Xunit;

namespace MemberAPIs.UnitTests.Constants;

public class LoginFailedTypeConstantsTests
{
    [Fact]
    public void IsValidLoginFailedType_WithChangedPassword_ReturnsTrue()
    {
        // Act
        var result = LoginFailedTypeConstants.IsValidLoginFailedType(LoginFailedTypeConstants.ChangedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidLoginFailedType_WithReachFailedCount_ReturnsTrue()
    {
        // Act
        var result = LoginFailedTypeConstants.IsValidLoginFailedType(LoginFailedTypeConstants.ReachFailedCount);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(100)]
    public void IsValidLoginFailedType_WithInvalidValues_ReturnsFalse(int invalidValue)
    {
        // Act
        var result = LoginFailedTypeConstants.IsValidLoginFailedType(invalidValue);

        // Assert
        Assert.False(result);
    }
} 