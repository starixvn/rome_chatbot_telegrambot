namespace TelegramBot.Importer.Configuration;

public sealed class AppSettings
{
    public string System { get; init; } = default!;

    public bool IsBotTest { get; init; }
}