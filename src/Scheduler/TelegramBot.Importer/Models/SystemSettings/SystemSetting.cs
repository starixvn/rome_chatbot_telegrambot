using System.Globalization;

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

    public bool ServiceFlag => GetBool(nameof(ServiceFlag));

    public int BatchSize => GetInt(nameof(BatchSize));

    public int MaxMsgTargetId => GetInt(nameof(MaxMsgTargetId));

    public int MaxLogId => GetInt(nameof(MaxLogId));

    public long MaxLogIdLong => GetLong(nameof(MaxLogId));

    public DateTime LastQueryTime => GetDateTime(nameof(LastQueryTime));

    public string LastDataRow => GetString(nameof(LastDataRow), string.Empty);

    public string ServerName => GetString(nameof(ServerName), string.Empty);

    private bool GetBool(string key)
        => "1" == GetString(key, "0");

    private int GetInt(string key)
        => int.Parse(GetString(key, "0"));

    private long GetLong(string key)
        => long.Parse(GetString(key, "0"));

    private DateTime GetDateTime(string key)
        => DateTime.Parse(GetString(key, DateTime.Now.ToString(CultureInfo.InvariantCulture)));

    private string GetString(
        string key,
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
