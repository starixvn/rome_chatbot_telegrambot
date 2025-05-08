using System;
using System.Threading.Tasks;
using FluentAssertions;
using MemberAPIs.Criteria;
using MemberAPIs.Constants;
using MemberAPIs.Models;
using MemberAPIs.Params;
using MemberAPIs.Services;
using Moq;
using Xunit;
using Fanex.Data.Repository;

namespace MemberAPIs.UnitTests
{
    public class LoginFailedNotifierServiceTests
    {
        private readonly Mock<IDynamicRepository> _mockDynamicRepository;
        private readonly Mock<ILoggerService> _mockLoggerService;
        private readonly LoginFailedNotifierService _service;

        public LoginFailedNotifierServiceTests()
        {
            _mockDynamicRepository = new Mock<IDynamicRepository>();
            _mockLoggerService = new Mock<ILoggerService>();
            _service = new LoginFailedNotifierService(_mockDynamicRepository.Object, _mockLoggerService.Object);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenFailedCountIsZero_ReturnsFailedResult()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ReachFailedCount
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 0
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
            result.ErrMsg.Should().Be(ApiMessagesConstants.LoginFailedCountZero);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenFailedCountExceedsSix_ReturnsFailedResult()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ReachFailedCount
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 7
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
            result.ErrMsg.Should().Be(ApiMessagesConstants.AccountIsLocked);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenUplineIdIsInvalid_ReturnsFailedResult()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ReachFailedCount
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 3,
                UplineId = 0
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                        criteria.UplineId = getCustInfoCriteria.UplineId;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
            result.ErrMsg.Should().Be($"Invalid UplineId for user {param.MemberId}: {getCustInfoCriteria.UplineId}");
            _mockLoggerService.Verify(x => x.Error(It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenFailedCountIsThree_ReturnsSuccess()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ReachFailedCount
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 3,
                UplineId = 456,
                UplineName = "Test Upline",
                CustName = "Test Customer"
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                        criteria.UplineId = getCustInfoCriteria.UplineId;
                        criteria.UplineName = getCustInfoCriteria.UplineName;
                        criteria.CustName = getCustInfoCriteria.CustName;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.Success);
            _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<InsertPersonalMessageCriteria>()), Times.Once);
            _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<AssignPersonalMessageCriteria>()), Times.Once);
            _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<InsertToastMessageCriteria>()), Times.Once);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenFailedCountIsNotThree_ReturnsFailedResult()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ReachFailedCount
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 4
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
            result.ErrMsg.Should().Be(ApiMessagesConstants.FailedCountNotThree);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenPasswordChanged_ReturnsSuccess()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 4,
                UplineId = 456,
                UplineName = "Test Upline",
                CustName = "Test Customer"
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                        criteria.UplineId = getCustInfoCriteria.UplineId;
                        criteria.UplineName = getCustInfoCriteria.UplineName;
                        criteria.CustName = getCustInfoCriteria.CustName;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.Success);
            _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<InsertPersonalMessageCriteria>()), Times.Once);
            _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<AssignPersonalMessageCriteria>()), Times.Once);
            _mockDynamicRepository.Verify(x => x.ExecuteAsync(It.IsAny<InsertToastMessageCriteria>()), Times.Once);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenLastChangedPasswordDateIsEmpty_ReturnsFailedResult()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                LastChangedPasswordDate = string.Empty
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 4
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
            result.ErrMsg.Should().Be(ApiMessagesConstants.LastChangedPasswordDateEmpty);
        }

        [Fact]
        public async Task AlertFailedLogin_WhenFailedCountIsThreeAndTypeIsChangedPassword_ReturnsFailedResult()
        {
            // Arrange
            var param = new AlertLoginFailedParam
            {
                MemberId = 123,
                LoginFailedType = LoginFailedTypeConstants.ChangedPassword,
                LastChangedPasswordDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var getCustInfoCriteria = new GetCustInfoCriteria(param.MemberId)
            {
                FailedCount = 3
            };

            _mockDynamicRepository
                .Setup(repo => repo.ExecuteAsync(It.IsAny<INonQueryCommand>()))
                .Returns(Task.FromResult(0))
                .Callback<INonQueryCommand>(cmd => 
                {
                    if (cmd is GetCustInfoCriteria criteria)
                    {
                        criteria.FailedCount = getCustInfoCriteria.FailedCount;
                    }
                });

            // Act
            var result = await _service.AlertFailedLogin(param);

            // Assert
            result.Should().NotBeNull();
            result.ErrCode.Should().Be(APIErrorCodeConstants.NotifyFailed);
            result.ErrMsg.Should().Be(ApiMessagesConstants.LoginFailedTypeMismatchForReach3Attempts);
        }
    }
} 