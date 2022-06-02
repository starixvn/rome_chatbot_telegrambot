using System;
using Fanex.Caching.Memory;
using Fanex.Caching.Redis;
using NSubstitute;
using StackExchange.Redis;
using TelegramBot.Infrastructure.Shared.Caching;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Shared.Caching;

public class CacheServiceFactoryTest
{
    private readonly CacheServiceFactory  cacheServiceFactory ;

    public CacheServiceFactoryTest()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        cacheServiceFactory = new CacheServiceFactory(serviceProvider);
    }

    [Fact]
    public void Get_NullOrDefault_ReturnMemoryCache()
    {
      
        var result= cacheServiceFactory.Get();
        Assert.IsType<MemoryCache>(result.CacheStorage);
        
        var result2= cacheServiceFactory.Get();
        Assert.Same(result,result2);
    } 
 
}