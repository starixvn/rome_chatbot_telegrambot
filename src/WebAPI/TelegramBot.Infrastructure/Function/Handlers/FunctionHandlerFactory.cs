using System;
using TelegramBot.Infrastructure.Function.Constants;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public interface IFunctionHandlerFactory
    {
        IFunctionHandler GetHandler(short functionId);
    }

    public class FunctionHandlerFactory : IFunctionHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public FunctionHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IFunctionHandler GetHandler(short functionId)
        {
            switch (functionId)
            {
                case FunctionIds.Oustanding:
                    return (IFunctionHandler)serviceProvider.GetService(typeof(OustandingFunctionHandler));

                case FunctionIds.TodayWinLoss:
                    return (IFunctionHandler)serviceProvider.GetService(typeof(TodayWinLossFunctionHandler));
                
                case FunctionIds.BalanceInfo:
                    return (IFunctionHandler)serviceProvider.GetService(typeof(BalanceInfoFunctionHandler));

                case FunctionIds.YesterdayWinLoss:
                    return (IFunctionHandler)serviceProvider.GetService(typeof(YesterdayWinLossFunctionHandler));

                default:
                    throw new NotImplementedException();
            }
        }
    }
}