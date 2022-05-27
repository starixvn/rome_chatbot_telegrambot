namespace TelegramBot.Importer.Constants;

public static class CustomerLevel
{
    public const byte Super = 4;
    public const byte Master = 3;
    public const byte Agent = 2;

    public static string GetName(byte custLevel)
        => custLevel switch
        {
            Super => nameof(Super),
            Master => nameof(Master),
            Agent => nameof(Agent),
            _ => throw new ArgumentOutOfRangeException(nameof(custLevel), $"Not expected custLevel value: {custLevel}")
        };
}