using Fanex.Logging;
using Moq;
using PortalAPIs.Services;

namespace PortalAPIs.UnitTests.Services;

public class LoggerServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ILoggerService _loggerService;

    public LoggerServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _loggerService = new LoggerService(_loggerMock.Object);
    }

    [Fact]
    public async Task Error_WhenRegularException_ShouldLogErrorWithExceptionDetails()
    {
        // Arrange
        var exception = new Exception("Test error message");

        // Act
        await _loggerService.Error(exception);

        // Assert
        _loggerMock.Verify(
            x => x.ErrorAsync(It.Is<string>(s =>
                s.Contains("Test error message") &&
                s.Contains("EXCEPTION:") &&
                s.Contains("System.Exception"))),
            Times.Once);
    }

    [Fact]
    public async Task Info_WhenMessageProvided_ShouldLogInfoMessage()
    {
        // Arrange
        var message = "Test info message";

        // Act
        await _loggerService.Info(message);

        // Assert
        _loggerMock.Verify(
            x => x.InfoAsync(message),
            Times.Once);
    }
}