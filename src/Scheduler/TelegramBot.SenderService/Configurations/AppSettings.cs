using System.Collections.Generic;

namespace TelegramBot.SenderService.Configurations;

public sealed class AppSettings
{
    public IDictionary<string, string> FunctionPermissions { get; init; } = default!;
    public bool IsTest { get; set; }
}