namespace TelegramBot.Importer.Services;

public interface ICacheRefService
{
    string GetProductName(int productId, string languageCode);

    string GetResourceDataByRefIdAndResourceId(string refId, string resourceId, string languageCode);
}

public class CacheRefService : ICacheRefService
{
    private readonly Fanex.RemotingRef.IRemotingRefService remotingRefService;

    public CacheRefService(Fanex.RemotingRef.IRemotingRefService remotingRefService)
    {
        this.remotingRefService = remotingRefService;
    }

    public string GetProductName(int productId, string languageCode)
        => GetResourceDataByRefIdAndResourceId("Product", productId.ToString(), languageCode);

    public string GetResourceDataByRefIdAndResourceId(string refId, string resourceId, string languageCode)
        => remotingRefService.GetResourceDataByRefIdAndResourceId(
            refId,
            resourceId,
            languageCode,
            resourceId);
}
