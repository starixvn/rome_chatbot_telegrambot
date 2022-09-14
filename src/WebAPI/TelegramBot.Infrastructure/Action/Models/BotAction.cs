namespace TelegramBot.Infrastructure.Action.Models;

public record BotAction
{
    public short ActionId { get; init; }
    public string ActionName { get; init; }
    public byte Type { get; init; }
    public string Command { get; init; }
    public short FunctionId { get; init; }
    public short ParentId { get; init; }
    public byte Status { get; init; }
    public bool IsEndAction { get; init; }
}