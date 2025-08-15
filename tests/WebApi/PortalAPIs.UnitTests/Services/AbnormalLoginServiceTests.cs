using Fanex.Data.Repository;
using Moq;
using PortalAPIs.Criteria;
using PortalAPIs.Models;
using PortalAPIs.Services;
using Xunit;

namespace PortalAPIs.UnitTests.Services
{
    public class AbnormalLoginServiceTests
    {
        private readonly Mock<IDynamicRepository> _mockDynamicRepository;
        private readonly Mock<ILoggerService> _mockLoggerService;
        private readonly IAbnormalLoginService _service;

        public AbnormalLoginServiceTests()
        {
            _mockDynamicRepository = new Mock<IDynamicRepository>();
            _mockLoggerService = new Mock<ILoggerService>();
            _service = new AbnormalLoginService(_mockDynamicRepository.Object, _mockLoggerService.Object);
        }

        [Fact]
        public async Task AlertAbnormalLogin_WhenPreprocessFails_ReturnsFalse()
        {
            // Arrange
            var trans = new List<AbnormalLoginModel>();
            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<PrepareAbnormalLoginInsertCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc) =>
                {
                    var c = criteria as PrepareAbnormalLoginInsertCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 1;
                    c.ErrMsg = "Test error";
                });

            // Act
            var result = await _service.AlertAbnormalLogin(trans);

            // Assert
            Assert.False(result);
            _mockLoggerService.Verify(x => x.Error(It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task AlertAbnormalLogin_WhenPreprocessSucceeds_ReturnsTrue()
        {
            // Arrange
            var trans = new List<AbnormalLoginModel>();
            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<PrepareAbnormalLoginInsertCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc) =>
                {
                    var c = criteria as PrepareAbnormalLoginInsertCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 0;
                    c.NewBatch = "test-batch";
                });

            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<AddAbnormalLoginCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc) =>
                {
                    var c = criteria as AddAbnormalLoginCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 0;
                });

            // Act
            var result = await _service.AlertAbnormalLogin(trans);

            // Assert
            Assert.True(result);
            _mockLoggerService.Verify(x => x.Error(It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task PreprocessData_WhenSuccessful_ReturnsValidResult()
        {
            // Arrange
            var trans = new List<AbnormalLoginModel>();
            var expectedBatch = "test-batch";
            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<PrepareAbnormalLoginInsertCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc) =>
                {
                    var c = criteria as PrepareAbnormalLoginInsertCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 0;
                    c.NewBatch = expectedBatch;
                });

            // Act
            var result = await ((AbnormalLoginService)_service).PreprocessData(trans);

            // Assert
            Assert.Equal(0, result.ErrCode);
            Assert.Equal(expectedBatch, result.Data);
        }

        [Fact]
        public async Task InsertData_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var batch = "test-batch";
            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<AddAbnormalLoginCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc)=>
                {
                    var c = criteria as AddAbnormalLoginCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 0;
                });

            // Act
            var result = await ((AbnormalLoginService)_service).InsertData(batch);

            // Assert
            Assert.True(result);
            _mockLoggerService.Verify(x => x.Error(It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task InsertData_WhenHasDuplicateIds_LogsInfoAndReturnsTrue()
        {
            // Arrange
            var batch = "test-batch";
            var duplicateIds = "123,456";
            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<AddAbnormalLoginCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc) =>
                {
                    var c = criteria as AddAbnormalLoginCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 0;
                    c.ExitsTransIDs = duplicateIds;
                });

            // Act
            var result = await ((AbnormalLoginService)_service).InsertData(batch);

            // Assert
            Assert.True(result);
            _mockLoggerService.Verify(x => x.Info($"Insert duplicate Ids: {duplicateIds}"), Times.Once);
        }

        [Fact]
        public async Task InsertData_WhenErrorOccurs_LogsErrorAndReturnsTrue()
        {
            // Arrange
            var batch = "test-batch";
            _mockDynamicRepository
                .Setup(x => x.ExecuteAsync(It.IsAny<AddAbnormalLoginCriteria>(), "", It.IsAny<CancellationToken>()))
                .Callback((INonQueryCommand criteria, string s, CancellationToken cc) =>
                {
                    var c = criteria as AddAbnormalLoginCriteria;
                    if (c == null)
                    {
                        return;
                    }

                    c.ErrCode = 1;
                    c.ErrMsg = "Test error";
                });

            // Act
            var result = await ((AbnormalLoginService)_service).InsertData(batch);

            // Assert
            Assert.True(result);
            _mockLoggerService.Verify(x => x.Error(It.IsAny<Exception>()), Times.Once);
        }
    }
} 