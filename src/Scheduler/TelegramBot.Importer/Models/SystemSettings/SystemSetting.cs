namespace TelegramBot.Importer.Models.SystemSettings;

public class SystemSetting
{
    private readonly IDictionary<string, string> settings;
    private readonly string environment;

    public SystemSetting() : this(new Dictionary<string, string>(), string.Empty)
    {
    }

    public SystemSetting(IDictionary<string, string> settings, string environment)
    {
        this.settings = settings;
        this.environment = environment;
    }

    public bool ServiceFlag => GetBool(settings, nameof(ServiceFlag), environment);

    public int BatchSize => GetInt(settings, nameof(BatchSize), environment);

    public int MaxMsgTargetId => GetInt(settings, nameof(MaxMsgTargetId), environment);

    public int MaxLogId => GetInt(settings, nameof(MaxLogId), environment);

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
