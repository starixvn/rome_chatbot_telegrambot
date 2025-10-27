using MemberAPIs.Services;
using Moq;

namespace MemberAPIs.UnitTests.Services;

public class LoggerServiceTests
{
    private readonly Mock<Fanex.Logging.ILogger> _loggerMock;
    private readonly LoggerService _sut;

    public LoggerServiceTests()
    {
        _loggerMock = new Mock<Fanex.Logging.ILogger>();
        _sut = new LoggerService(_loggerMock.Object);
    }

    [Fact]
    public async Task Error_WithRegularException_LogsErrorWithMessageAndStackTrace()
    {
        // Arrange
        var exception = new Exception("Test error message");
        _loggerMock.Setup(x => x.ErrorAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Error(exception);

        // Assert
        _loggerMock.Verify(x => x.ErrorAsync(It.Is<string>(s =>
            s.Contains("Test error message") &&
            s.Contains("EXCEPTION:") &&
            s.Contains("System.Exception"))), Times.Once);
    }

    [Fact]
    public async Task Info_LogsInfoMessage()
    {
        // Arrange
        const string message = "Test info message";
        _loggerMock.Setup(x => x.InfoAsync(message))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Info(message);

        // Assert
        _loggerMock.Verify(x => x.InfoAsync(message), Times.Once);
    }
}