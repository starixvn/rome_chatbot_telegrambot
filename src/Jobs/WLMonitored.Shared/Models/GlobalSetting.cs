namespace WLMonitored.Shared.Models;

public sealed class GlobalSetting
{
    private const string WLMonitoredAccount = "WLMonitoredAccount";
    private const string Dep_WLMonitoredAccount = "Dep_WLMonitoredAccount";
    private const string SuperUUS = "SuperUUS";

    private readonly IDictionary<string, string> settings;
    private readonly string environment;

    public GlobalSetting(IDictionary<string, string> settings, string environment)
    {
        this.settings = settings;
        this.environment = environment;
    }

    public bool IsStopJob(string jobName)
        => !GetBool(environment, WLMonitoredAccount, jobName);

    public long LastId
        => GetLong(environment, WLMonitoredAccount, nameof(LastId));

    public DateTime LastCheckTime
        => GetDateTime(environment, WLMonitoredAccount, nameof(LastCheckTime));

    public DateTime LastScanSettingDate
        => GetDateTime(environment, WLMonitoredAccount, nameof(LastScanSettingDate));

    public string CreditSuperUUS
        => GetString(string.Empty, WLMonitoredAccount, SuperUUS);

    public string DepositSuperUUS
        => GetString(string.Empty, Dep_WLMonitoredAccount, SuperUUS);

    private bool GetBool(string env, string serviceName, string settingName)
        => "1" == GetString(env, serviceName, settingName);

    private long GetLong(string env, string serviceName, string settingName)
        => long.Parse(GetString(env, serviceName, settingName));

    private DateTime GetDateTime(string env, string serviceName, string settingName)
        => DateTime.Parse(GetString(env, serviceName, settingName));

    private string GetString(string env, string serviceName, string settingName)
        => settings[string.IsNullOrWhiteSpace(env)
            ? $"{serviceName}_{settingName}"
            : $"{env}-{serviceName}_{settingName}"];

    public string GetWLMonitoredAccountWithEnvironment()
        => string.IsNullOrWhiteSpace(environment)
            ? WLMonitoredAccount
            : $"{environment}-{WLMonitoredAccount}";
}