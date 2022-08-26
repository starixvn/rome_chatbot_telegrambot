namespace TelegramBot.SenderService.Configurations;

public class AppSettings
{
    public IDictionary<string, string> FunctionPermissions { get; init; } = default!;
    public bool IsTest { get; set; }
    public string ProxyAddress { get; set; }
}