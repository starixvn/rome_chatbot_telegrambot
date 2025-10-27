using Fanex.Caching;
using Fanex.Caching.Memory;
using Fanex.Caching.Redis;
using Moq;
using PortalAPIs.Services;

namespace PortalAPIs.UnitTests.Services
{
    public class CacheServiceFactoryTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ICache> _memoryCacheMock;
        private readonly Mock<ICache> _redisCacheMock;

        public CacheServiceFactoryTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _memoryCacheMock = new Mock<ICache>();
            _redisCacheMock = new Mock<ICache>();

            _serviceProviderMock.Setup(x => x.GetService(typeof(MemoryCache)))
                .Returns(_memoryCacheMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(RedisCache)))
                .Returns(_redisCacheMock.Object);
        }

        [Fact]
        public void Get_WhenUsingMemoryProvider_ReturnsMemoryCacheService()
        {
            // Arrange
            var factory = new CacheServiceFactory(_serviceProviderMock.Object);

            // Act
            var result = factory.Get(CacheProvider.Memory);

            // Assert
            Assert.NotNull(result);
            _serviceProviderMock.Verify(x => x.GetService(typeof(MemoryCache)), Times.Once);
        }

        [Fact]
        public void Get_WhenUsingRedisProvider_ReturnsRedisCacheService()
        {
            // Arrange
            var factory = new CacheServiceFactory(_serviceProviderMock.Object);

            // Act
            var result = factory.Get(CacheProvider.Redis);

            // Assert
            Assert.NotNull(result);
            _serviceProviderMock.Verify(x => x.GetService(typeof(RedisCache)), Times.Once);
        }

        [Fact]
        public void Get_WhenCalledMultipleTimesWithSameProvider_ReturnsSameInstance()
        {
            // Arrange
            var factory = new CacheServiceFactory(_serviceProviderMock.Object);

            // Act
            var result1 = factory.Get(CacheProvider.Memory);
            var result2 = factory.Get(CacheProvider.Memory);

            // Assert
            Assert.Same(result1, result2);
            _serviceProviderMock.Verify(x => x.GetService(typeof(MemoryCache)), Times.Once);
        }

        [Fact]
        public void Get_WhenNoProviderSpecified_ReturnsMemoryCacheService()
        {
            // Arrange
            var factory = new CacheServiceFactory(_serviceProviderMock.Object);

            // Act
            var result = factory.Get();

            // Assert
            Assert.NotNull(result);
            _serviceProviderMock.Verify(x => x.GetService(typeof(MemoryCache)), Times.Once);
        }
    }
}