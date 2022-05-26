using System;
using System.Threading.Tasks;
using Alpha.API.Client.Services;
using NSubstitute;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Shared.Configurations;
using Xunit;

namespace TelegramBot.Infrastructure.UnitTests.Function.Handlers
{
    public class FunctionHandlerFactoryTest
    {
        private readonly IServiceProvider serviceProvider;
        private readonly FunctionHandlerFactory functionHandlerFactory;

        public FunctionHandlerFactoryTest()
        {
            serviceProvider = Substitute.For<IServiceProvider>();
            functionHandlerFactory = new FunctionHandlerFactory(serviceProvider);
        }

        [Fact]
        public async Task GetHandler_FunctionIdOustanding_ReturnOustandingFunctionHandler()
        {
            serviceProvider.GetService(typeof(OustandingFunctionHandler))
                .Returns(new OustandingFunctionHandler(
                    Substitute.For<IMessageBuilder>(), 
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IOutstandingService>(), 
                    Substitute.For<ICustomerService>(),
                    Substitute.For<WebAPIConfiguration>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.Oustanding);
            Assert.Equal(typeof(OustandingFunctionHandler), result.GetType());
        }
        
        [Fact]
        public async Task GetHandler_FunctionIdTodayWinLoss_ReturnTodayWinLossFunctionHandler()
        {
            serviceProvider.GetService(typeof(TodayWinLossFunctionHandler))
                .Returns(new TodayWinLossFunctionHandler(
                    Substitute.For<IMessageBuilder>(), 
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IWinLossService>(), 
                    Substitute.For<ICustomerService>(),
                    Substitute.For<WebAPIConfiguration>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.TodayWinLoss);
            Assert.Equal(typeof(TodayWinLossFunctionHandler), result.GetType());
        }
        
        [Fact]
        public async Task GetHandler_FunctionIdBalanceInfo_ReturnBalanceInfoFunctionHandler()
        {
            serviceProvider.GetService(typeof(BalanceInfoFunctionHandler))
                .Returns(new BalanceInfoFunctionHandler(
                    Substitute.For<IMessageBuilder>(),
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<ICustomerService>(),
                    Substitute.For<IBalanceInfoService>(),
                    Substitute.For<WebAPIConfiguration>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.BalanceInfo);
            Assert.Equal(typeof(BalanceInfoFunctionHandler), result.GetType());
        }
        
        [Fact]
        public async Task GetHandler_FunctionYesterdayWinLoss_ReturnYesterdayWinLossFunctionHandler()
        {
            serviceProvider.GetService(typeof(YesterdayWinLossFunctionHandler))
                .Returns(new YesterdayWinLossFunctionHandler(
                    Substitute.For<IMessageBuilder>(), 
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IWinLossService>(), 
                    Substitute.For<ICustomerService>(),
                    Substitute.For<WebAPIConfiguration>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.YesterdayWinLoss);
            Assert.Equal(typeof(YesterdayWinLossFunctionHandler), result.GetType());
        }
        
        [Fact]
        public async Task GetHandler_UnknownFunctionID_ThrowException()
        {
            try
            {
                functionHandlerFactory.GetHandler(9999);
            }
            catch (Exception e)
            {      
                Assert.Equal(typeof(NotImplementedException), e.GetType());
            }
        }
        
    }
}