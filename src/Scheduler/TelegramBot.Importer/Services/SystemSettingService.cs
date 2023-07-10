using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.SystemSettings;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.SystemSettings;

namespace TelegramBot.Importer.Services;

public interface ISystemSettingService
{
    Task<SystemSetting> GetSystemSetting(string serviceName, string environment);

    Task UpdateJobStatusAndLastRunTime(
        string serviceName,
        string environment,
        string jobStatus,
        DateTime lastRuntime);

    Task UpdateJobStatus(
        string serviceName,
        string environment,
        string jobStatus,
        DateTime lastRuntime,
        long maxLogId);
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

    public Task UpdateJobStatusAndLastRunTime(
        string serviceName,
        string environment,
        string jobStatus,
        DateTime lastRuntime)
        => dynamicRepository.ExecuteAsync(
            new UpdateSystemSettingCommand(serviceName, environment, BuildSettingXML(jobStatus, lastRuntime)));

    public Task UpdateJobStatus(
        string serviceName,
        string environment,
        string jobStatus,
        DateTime lastRuntime,
        long maxLogId)
        => dynamicRepository.ExecuteAsync(
            new UpdateSystemSettingCommand(serviceName, environment, BuildSettingXML(jobStatus, lastRuntime, maxLogId)));

    private static string BuildSettingXML(string jobStatus, DateTime lastRuntime)
    {
        var setting = new StringBuilder();
        setting.Append("<root>")
            .Append($"<r SettingName=\"JobStatus\" Value=\"{jobStatus}\"/>")
            .Append($"<r SettingName=\"LastRuntime\" Value=\"{lastRuntime.ToLongDateTime24h()}\"/>")
            .Append("</root>");

        return setting.ToString();
    }

    private static string BuildSettingXML(string jobStatus, DateTime lastRuntime, long maxLogId)
    {
        var setting = new StringBuilder();
        setting.Append("<root>")
            .Append($"<r SettingName=\"JobStatus\" Value=\"{jobStatus}\"/>")
            .Append($"<r SettingName=\"LastRuntime\" Value=\"{lastRuntime.ToLongDateTime24h()}\"/>")
            .Append($"<r SettingName=\"MaxLogId\" Value=\"{maxLogId}\"/>")
            .Append("</root>");

        return setting.ToString();
    }
}
