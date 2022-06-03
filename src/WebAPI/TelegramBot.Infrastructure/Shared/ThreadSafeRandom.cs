namespace TelegramBot.Common.Extensions;

public static class ThreadSafeRandom
{
    [ThreadStatic]
    private static Random _local;

    public static int Next(int minValue, int maxValue)
    {
        Random inst = _local;
        if (inst == null)
        {
            _local = inst = new Random();
        }

        return inst.Next(minValue, maxValue);
    }
}

