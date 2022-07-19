namespace TelegramBot.Infrastructure.Shared.Models;

public record Messenger(int BotId, long ChatId, string System = "") { }
