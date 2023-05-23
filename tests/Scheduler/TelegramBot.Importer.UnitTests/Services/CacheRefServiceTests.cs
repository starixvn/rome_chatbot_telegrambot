using Fanex.RemotingRef;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class CacheRefServiceTests
{
    private readonly IRemotingRefService remotingRefService;
    private readonly CacheRefService cacheRefService;

    public CacheRefServiceTests()
    {
        remotingRefService = Substitute.For<IRemotingRefService>();
        cacheRefService = new CacheRefService(remotingRefService);
    }

    [Theory, AutoData]
    public void GetProductName_Always_CallsErrorAsync(int productId, string languageCode)
    {
        // Act
        cacheRefService.GetProductName(productId, languageCode);

        // Assert
        remotingRefService.Received().GetResourceDataByRefIdAndResourceId("Product", productId.ToString(), languageCode, productId.ToString());
    }
}