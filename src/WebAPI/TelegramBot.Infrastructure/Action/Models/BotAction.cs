namespace TelegramBot.Infrastructure.Action.Models;

public record BotAction(
    short ActionId,
    string ActionName,
    byte Type,
    string Command,
    short FunctionId,
    short ParentId,
    byte Status,
    bool IsEndAction)
{ };