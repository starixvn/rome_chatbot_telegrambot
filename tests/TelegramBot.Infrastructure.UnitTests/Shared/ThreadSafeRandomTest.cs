using TelegramBot.Common.Extensions;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Shared;

public class ThreadSafeRandomTest
{
    [Fact]
    public void Next()
    {
        var result=  ThreadSafeRandom.Next(1, 3);
        Assert.InRange(result ,1,3);
        
        var result2=  ThreadSafeRandom.Next(1, 3);
        Assert.InRange(result2 ,1,3);
    }
}