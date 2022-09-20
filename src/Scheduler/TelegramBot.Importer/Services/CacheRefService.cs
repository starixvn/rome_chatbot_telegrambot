using Fanex.RemotingRef;

namespace TelegramBot.Importer.Services;

public interface ICacheRefService
{
    string GetProductName(int productId, string languageCode);
}

public class CacheRefService : ICacheRefService
{
    private readonly IRemotingRefService remotingRefService;

    public CacheRefService(IRemotingRefService remotingRefService)
    {
        this.remotingRefService = remotingRefService;
    }

    public string GetProductName(int productId, string languageCode)
        => GetResourceDataByRefIdAndResourceId("Product", productId.ToString(), languageCode);

    private string GetResourceDataByRefIdAndResourceId(string refId, string resourceId, string languageCode)
        => remotingRefService.GetResourceDataByRefIdAndResourceId(
            refId,
            resourceId,
            languageCode,
            resourceId);
}
