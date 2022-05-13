namespace TelegramBot.SenderService.Configurations;

public sealed class FunctionPermission
{
    public string FunctionKey { get; init; } = default!;
    
    public string SubAccPermission { get; init; } = default!;
}