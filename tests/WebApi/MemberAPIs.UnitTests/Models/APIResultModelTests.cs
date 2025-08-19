using FluentAssertions;
using MemberAPIs.Constants;
using MemberAPIs.Models;
using Xunit;

namespace MemberAPIs.UnitTests.Models;

public class APIResultModelTests
{
    [Fact]
    public void Constructor_Default_ShouldInitializeWithSuccessValues()
    {
        // Act
        var result = new APIResultModel();

        // Assert
        result.ErrCode.Should().Be(APIErrorCodeConstants.Success);
        result.ErrMsg.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldInitializeWithProvidedValues()
    {
        // Arrange
        const string errorCode = "123";
        const string errorMessage = "Test error message";

        // Act
        var result = new APIResultModel(errorCode, errorMessage);

        // Assert
        result.ErrCode.Should().Be(errorCode);
        result.ErrMsg.Should().Be(errorMessage);
    }

    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        // Act
        var result = APIResultModel.Success();

        // Assert
        result.ErrCode.Should().Be(APIErrorCodeConstants.Success);
        result.ErrMsg.Should().Be(string.Empty);
    }

    [Fact]
    public void Failed_WithOnlyErrorMessage_ShouldReturnFailedResultWithDefaultErrorCode()
    {
        // Arrange
        const string errorMessage = "Test error message";

        // Act
        var result = APIResultModel.Failed(errorMessage);

        // Assert
        result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
        result.ErrMsg.Should().Be(errorMessage);
    }

    [Fact]
    public void Failed_WithErrorMessageAndErrorCode_ShouldReturnFailedResultWithProvidedValues()
    {
        // Arrange
        const string errorCode = "123";
        const string errorMessage = "Test error message";

        // Act
        var result = APIResultModel.Failed(errorMessage, errorCode);

        // Assert
        result.ErrCode.Should().Be(errorCode);
        result.ErrMsg.Should().Be(errorMessage);
    }
}