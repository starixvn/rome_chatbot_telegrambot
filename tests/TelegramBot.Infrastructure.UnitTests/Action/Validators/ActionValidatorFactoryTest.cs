using System;
using NSubstitute;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Validators;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Action.Validators;

public class ActionValidatorFactoryTest
{
    private readonly IServiceProvider serviceProvider;
    private readonly ActionValidatorFactory  actionValidatorFactory ;
    
    public ActionValidatorFactoryTest()
    {
        serviceProvider = Substitute.For<IServiceProvider>();
        actionValidatorFactory = new ActionValidatorFactory(serviceProvider);
    }

    [Fact]
    public void GetValidator_OutstandingEnterDownlineUsername_ReturnDirectDownlinesValidator()
    {
       var result= actionValidatorFactory.GetValidator(ActionIds.OutstandingEnterDownlineUsername);
       Assert.Equal(typeof(DirectDownlinesValidator), result.GetType());
    } 
    
    [Fact]
    public void GetValidator_BalanceEnterDownlineUsername_ReturnDirectDownlinesValidator()
    {
       var result= actionValidatorFactory.GetValidator(ActionIds.BalanceEnterDownlineUsername);
       Assert.Equal(typeof(DirectDownlinesValidator), result.GetType());
    }  
    
    [Fact]
    public void GetValidator_TodayWinLossDownlineUsername_ReturnDirectDownlinesValidator()
    {
       var result= actionValidatorFactory.GetValidator(ActionIds.TodayWinLossDownlineUsername);
       Assert.Equal(typeof(DirectDownlinesValidator), result.GetType());
    }
    
    [Fact]
    public void GetValidator_YesterdayWinLossDownlineUsername_ReturnDirectDownlinesValidator()
    {
       var result= actionValidatorFactory.GetValidator(ActionIds.YesterdayWinLossDownlineUsername);
       Assert.Equal(typeof(DirectDownlinesValidator), result.GetType());
    }
    
    [Fact]
    public void GetValidator_TopXOutstanding_ReturnNumberOfDownlinesValidator()
    {
       serviceProvider.GetService(typeof(WebAPIConfiguration))
          .Returns(new WebAPIConfiguration() { OutstandingMaxTopQuery = 10 });
       
       var result= actionValidatorFactory.GetValidator(ActionIds.TopXOutstanding);
       
       Assert.Equal(typeof(NumberOfDownlinesValidator), result.GetType());
    } 
    
    [Fact]
    public void GetValidator_TopXBalance_ReturnNumberOfDownlinesValidator()
    {
       serviceProvider.GetService(typeof(WebAPIConfiguration))
          .Returns(new WebAPIConfiguration() { BalanceMaxTopQuery = 10 });
       
       var result= actionValidatorFactory.GetValidator(ActionIds.TopXBalance);
       
       Assert.Equal(typeof(NumberOfDownlinesValidator), result.GetType());
    } 
    
    [Fact]
    public void GetValidator_Default_ReturnNull()
    {
       var result= actionValidatorFactory.GetValidator(999);
       Assert.Equal(null, result);
    }
}