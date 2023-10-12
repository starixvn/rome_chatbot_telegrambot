using System;
using Alpha.API.Client.Services;
using Fanex.Data.Repository;
using NSubstitute;
using TelegramBot.Modules.Function.Constants;
using TelegramBot.Modules.Function.Handlers;
using TelegramBot.Modules.Message;
using TelegramBot.Shared.Configurations;
using Xunit;

namespace TelegramBot.Modules.UnitTests.Function.Handlers
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
        public void GetHandler_FunctionIdOutstanding_ReturnOutstandingFunctionHandler()
        {
            serviceProvider.GetService(typeof(OutstandingFunctionHandler))
                .Returns(new OutstandingFunctionHandler(
                    Substitute.For<IMessageBuilder>(),
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IOutstandingService>(),
                    Substitute.For<ICustomerService>(),
                    Substitute.For<WebAPIConfiguration>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.Outstanding);
            Assert.Equal(typeof(OutstandingFunctionHandler), result.GetType());
        }

        [Fact]
        public void GetHandler_FunctionIdTodayWinLoss_ReturnTodayWinLossFunctionHandler()
        {
            serviceProvider.GetService(typeof(TodayWinLossFunctionHandler))
                .Returns(new TodayWinLossFunctionHandler(
                    Substitute.For<IMessageBuilder>(),
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IWinLossService>(),
                    Substitute.For<ICustomerService>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.TodayWinLoss);
            Assert.Equal(typeof(TodayWinLossFunctionHandler), result.GetType());
        }

        [Fact]
        public void GetHandler_FunctionIdBalanceInfo_ReturnBalanceInfoFunctionHandler()
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
        public void GetHandler_FunctionYesterdayWinLoss_ReturnYesterdayWinLossFunctionHandler()
        {
            serviceProvider.GetService(typeof(YesterdayWinLossFunctionHandler))
                .Returns(new YesterdayWinLossFunctionHandler(
                    Substitute.For<IMessageBuilder>(),
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IWinLossService>(),
                    Substitute.For<ICustomerService>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.YesterdayWinLoss);
            Assert.Equal(typeof(YesterdayWinLossFunctionHandler), result.GetType());
        }

        [Fact]
        public void GetHandler_FunctionAvailableDomains_ReturnAgencyDomainsFunctionHandler()
        {
            serviceProvider.GetService(typeof(AgencyDomainsFunctionHandler))
                .Returns(new AgencyDomainsFunctionHandler(
                    Substitute.For<IMessageBuilder>(),
                    Substitute.For<IMessageFormatter>(),
                    Substitute.For<IDynamicRepository>()));
            var result = functionHandlerFactory.GetHandler(FunctionIds.AgencyDomains);
            Assert.Equal(typeof(AgencyDomainsFunctionHandler), result.GetType());
        }

        [Fact]
        public void GetHandler_UnknownFunctionID_ThrowException()
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