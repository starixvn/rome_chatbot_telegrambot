using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MemberAPIs.Controllers;
using MemberAPIs.Services;
using MemberAPIs.Requests;
using MemberAPIs.Params;
using MemberAPIs.Configurations;
using MemberAPIs.Constants;
using MemberAPIs.Models;
using FluentAssertions;
using Xunit;

namespace MemberAPIs.UnitTests.Controllers
{
    public class LoginFailedNotifierControllerTests
    {
        private readonly Mock<ILoginFailedNotifierService> _serviceMock;
        private readonly Mock<ILogger<LoginFailedNotifierController>> _loggerMock;
        private readonly LoginFailedNotifierController _controller;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<HttpResponse> _responseMock;

        public LoginFailedNotifierControllerTests()
        {
            _serviceMock = new Mock<ILoginFailedNotifierService>();
            _loggerMock = new Mock<ILogger<LoginFailedNotifierController>>();
            _controller = new LoginFailedNotifierController(_serviceMock.Object, _loggerMock.Object);

            // Setup HttpContext and Response
            _httpContextMock = new Mock<HttpContext>();
            _responseMock = new Mock<HttpResponse>();

            // Setup Response.StatusCode property
            var statusCode = 0;
            _responseMock.SetupProperty(r => r.StatusCode, statusCode);

            _httpContextMock.Setup(m => m.Response).Returns(_responseMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        [Fact]
        public async Task InsertNotification_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var apiKey = AppSetting.APIToken;
            var request = new AlertLoginFailedRequest
            {
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                MemberId = 123,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var expectedResponse = new APIResultModel { ErrCode = APIErrorCodeConstants.Success };
            _serviceMock.Setup(x => x.AlertFailedLogin(It.IsAny<AlertLoginFailedParam>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.InsertNotification(apiKey, request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
            _serviceMock.Verify(x => x.AlertFailedLogin(It.Is<AlertLoginFailedParam>(p =>
                p.LoginFailedType == request.LoginFailedType &&
                p.MemberId == request.MemberId &&
                p.LastChangedPasswordDate == request.LastChangedPasswordDate)), Times.Once);
        }

        [Fact]
        public async Task InsertNotification_WithMissingApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var request = new AlertLoginFailedRequest
            {
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                MemberId = 123,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Act
            var result = await _controller.InsertNotification(string.Empty, request);

            // Assert
            result.Should().BeEquivalentTo(ApiMessagesConstants.ApiKeyRequired);
            _controller.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            _serviceMock.Verify(x => x.AlertFailedLogin(It.IsAny<AlertLoginFailedParam>()), Times.Never);
        }

        [Fact]
        public async Task InsertNotification_WithInvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var request = new AlertLoginFailedRequest
            {
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                MemberId = 123,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Act
            var result = await _controller.InsertNotification("invalid-key", request);

            // Assert
            result.Should().BeEquivalentTo(ApiMessagesConstants.ApiKeyInvalid);
            _controller.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            _serviceMock.Verify(x => x.AlertFailedLogin(It.IsAny<AlertLoginFailedParam>()), Times.Never);
        }

        [Fact]
        public async Task InsertNotification_WithInvalidLoginFailedType_ReturnsBadRequest()
        {
            // Arrange
            var apiKey = AppSetting.APIToken;
            var request = new AlertLoginFailedRequest
            {
                LoginFailedType = 999, // Invalid type
                MemberId = 123,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Act
            var result = await _controller.InsertNotification(apiKey, request);

            // Assert
            result.Should().BeEquivalentTo(ApiMessagesConstants.InvalidLoginFailedType);
            _controller.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            _serviceMock.Verify(x => x.AlertFailedLogin(It.IsAny<AlertLoginFailedParam>()), Times.Never);
        }

        [Fact]
        public async Task InsertNotification_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var apiKey = AppSetting.APIToken;
            var request = new AlertLoginFailedRequest
            {
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                MemberId = 123,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _serviceMock.Setup(x => x.AlertFailedLogin(It.IsAny<AlertLoginFailedParam>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.InsertNotification(apiKey, request);

            // Assert
            result.Should().BeEquivalentTo(ApiMessagesConstants.InternalServerError);
            _controller.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}