namespace TelegramBot.SenderService.Models;

public sealed class Notification
{
    public int NotificationId { get; init; }

    public int UserId { get; init; }

    public int CustId { get; init; }

    public int SubAccId { get; init; }

    public string Message { get; init; } = default!;

    public int BotId { get; init; }

    public long ChatId { get; init; }

    public string FunctionKey { get; init; } = default!;

    public string Status { get; set; } = default!;

    public string ErrorMessage { get; set; } = default!;
}
