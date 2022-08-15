namespace TelegramBot.Importer.Models.Announcement;

public class MessageContent
{
    public string LanguageKey { get; set; } = default!;

    public string Text { get; set; } = default!;

    public bool IsDisplayingByUrl { get; set; }

    public string Url { get; set; } = default!;
}