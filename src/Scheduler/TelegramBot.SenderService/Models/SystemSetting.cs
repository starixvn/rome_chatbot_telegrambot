using System.Collections.Generic;

namespace TelegramBot.SenderService.Models;

public sealed class SystemSetting
{
    public SystemSetting() : this(new Dictionary<string, string>(), string.Empty)
    {
    }

    public SystemSetting(IDictionary<string, string> settings, string environment)
    {
        ServiceFlag = GetBool(settings, nameof(ServiceFlag), environment);
        BatchSize = GetInt(settings, nameof(BatchSize), environment);
    }

    public bool ServiceFlag { get; }

    public int BatchSize { get; }

    private static bool GetBool(IDictionary<string, string> settings, string key, string environment)
        => "1" == GetString(settings, key, environment, "0");

    private static int GetInt(IDictionary<string, string> settings, string key, string environment)
        => int.Parse(GetString(settings, key, environment, "0"));

    private static string GetString(
        IDictionary<string, string> settings,
        string key,
        string environment,
        string defaultValue)
    {
        if (settings.TryGetValue($"{key}{environment}", out var value))
        {
            return value;
        }

        return settings.TryGetValue(key, out value)
            ? value
            : defaultValue;
    }
}
