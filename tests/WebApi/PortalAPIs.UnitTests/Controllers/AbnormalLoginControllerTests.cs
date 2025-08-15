using Moq;
using Newtonsoft.Json;
using PortalAPIs.Controllers;
using PortalAPIs.Models;
using PortalAPIs.Params;
using PortalAPIs.Services;

namespace PortalAPIs.UnitTests.Controllers
{
    public class AbnormalLoginControllerTests
    {
        private readonly Mock<IAbnormalLoginService> _mockService;
        private readonly AbnormalLoginController _controller;

        public AbnormalLoginControllerTests()
        {
            _mockService = new Mock<IAbnormalLoginService>();
            _controller = new AbnormalLoginController(_mockService.Object);
        }

        [Fact]
        public async Task AlertAbnormalLogin_WhenValidDataProvided_ReturnsTrue()
        {
            // Arrange
            var abnormalLoginParams = new List<AbnormalLoginParam>
            {
                new AbnormalLoginParam
                {
                    ID = 1,
                    LoginName = "testUser",
                    TransTime = DateTime.Now,
                    UserAgent = "Mozilla/5.0",
                    IP = "192.168.1.1",
                    OS = "Windows",
                    Device = "Desktop",
                    Browser = "Chrome"
                }
            };

            var jsonInput = JsonConvert.SerializeObject(abnormalLoginParams);
            _mockService.Setup(x => x.AlertAbnormalLogin(It.IsAny<IEnumerable<AbnormalLoginModel>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AlertAbnormalLogin(jsonInput);

            // Assert
            Assert.True(result);
            _mockService.Verify(x => x.AlertAbnormalLogin(It.IsAny<IEnumerable<AbnormalLoginModel>>()), Times.Once);
        }

        [Fact]
        public async Task AlertAbnormalLogin_WhenInvalidJsonProvided_ThrowsException()
        {
            // Arrange
            var invalidJson = "invalid json";

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(() => _controller.AlertAbnormalLogin(invalidJson));
            _mockService.Verify(x => x.AlertAbnormalLogin(It.IsAny<IEnumerable<AbnormalLoginModel>>()), Times.Never);
        }

        [Fact]
        public async Task AlertAbnormalLogin_WhenServiceReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var abnormalLoginParams = new List<AbnormalLoginParam>
            {
                new AbnormalLoginParam
                {
                    ID = 1,
                    LoginName = "testUser",
                    TransTime = DateTime.Now,
                    UserAgent = "Mozilla/5.0",
                    IP = "192.168.1.1",
                    OS = "Windows",
                    Device = "Desktop",
                    Browser = "Chrome"
                }
            };

            var jsonInput = JsonConvert.SerializeObject(abnormalLoginParams);
            _mockService.Setup(x => x.AlertAbnormalLogin(It.IsAny<IEnumerable<AbnormalLoginModel>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AlertAbnormalLogin(jsonInput);

            // Assert
            Assert.False(result);
            _mockService.Verify(x => x.AlertAbnormalLogin(It.IsAny<IEnumerable<AbnormalLoginModel>>()), Times.Once);
        }
    }
} 