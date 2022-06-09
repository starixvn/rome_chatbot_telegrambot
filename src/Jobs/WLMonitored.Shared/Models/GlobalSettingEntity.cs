namespace WLMonitored.Shared.Models;

public record GlobalSettingEntity(string ServiceName, string SettingName, string SettingValue)
{
    public string Key => $"{ServiceName}_{SettingName}";
}
