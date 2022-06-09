using Fanex.Data.Repository;
using WLMonitored.Shared.Criteria;
using WLMonitored.Shared.Models;

namespace WLMonitored.Shared.Services;

public interface IGlobalSettingService
{
    Task<GlobalSetting> GetGlobalSetting(string environment);
}

public sealed class GlobalSettingService : IGlobalSettingService
{
    private readonly IDynamicRepository dynamicRepository;

    public GlobalSettingService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<GlobalSetting> GetGlobalSetting(string environment)
    {
        var settings = await dynamicRepository.FetchAsync<GlobalSettingEntity>(new GetGlobalSettingCriteria());

        return new GlobalSetting(ToDictionary(settings), environment);
    }

    private static IDictionary<string, string> ToDictionary(IEnumerable<GlobalSettingEntity> settings)
    {
        var settingsDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in settings)
        {
            settingsDict[item.Key] = item.SettingValue;
        }

        return settingsDict;
    }
}