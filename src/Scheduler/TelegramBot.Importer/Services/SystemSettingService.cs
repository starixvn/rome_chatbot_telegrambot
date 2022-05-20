using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.SystemSettings;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.SystemSettings;

namespace TelegramBot.Importer.Services;

public interface ISystemSettingService
{
    Task<SystemSetting> GetSystemSetting(string serviceName, string environment);

    Task UpdateJobStatusToRunningdAndLastRunTime(string serviceName, string environment);
}

public class SystemSettingService : ISystemSettingService
{
    private readonly IDynamicRepository dynamicRepository;

    public SystemSettingService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<SystemSetting> GetSystemSetting(string serviceName, string environment)
    {
        var settings = await dynamicRepository.FetchAsync<SystemSettingEntity>(
            new GetSystemSettingCriteria(serviceName, environment));

        return new SystemSetting(ToDictionary(settings), environment);
    }

    private static IDictionary<string, string> ToDictionary(IEnumerable<SystemSettingEntity> settings)
    {
        var settingsDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        settings.ToList().ForEach(item => settingsDict[item.SettingName] = item.Value);

        return settingsDict;
    }

    public Task UpdateJobStatusToRunningdAndLastRunTime(string serviceName, string environment)
        => dynamicRepository.ExecuteAsync(
            new UpdateSystemSettingCriteria(serviceName, environment, BuildSettingXML()));

    private static string BuildSettingXML()
    {
        var lastRuntime = DateTime.Now.ToLongDateTime12h();

        var setting = new StringBuilder();
        setting.Append("<root>")
            .Append($"<r SettingName=\"JobStatus\" Value=\"Running\"/>")
            .Append($"<r SettingName=\"LastRuntime\" Value=\"{lastRuntime}\"/>")
            .Append("</root>");

        return setting.ToString();
    }
}
