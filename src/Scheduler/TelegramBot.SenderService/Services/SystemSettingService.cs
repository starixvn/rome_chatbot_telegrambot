using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TelegramBot.SenderService.Criteria;
using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.Services;

public interface ISystemSettingService
{
    Task<SystemSetting> GetSystemSetting(string serviceName, string environment);
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
}
