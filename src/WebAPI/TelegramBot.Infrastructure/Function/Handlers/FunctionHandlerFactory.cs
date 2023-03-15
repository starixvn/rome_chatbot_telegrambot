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
            => this.serviceProvider = serviceProvider;

        public IFunctionHandler GetHandler(short functionId)
        {
            return functionId switch
            {
                FunctionIds.Outstanding => (IFunctionHandler)serviceProvider.GetService(typeof(OutstandingFunctionHandler)),
                FunctionIds.TodayWinLoss => (IFunctionHandler)serviceProvider.GetService(typeof(TodayWinLossFunctionHandler)),
                FunctionIds.BalanceInfo => (IFunctionHandler)serviceProvider.GetService(typeof(BalanceInfoFunctionHandler)),
                FunctionIds.YesterdayWinLoss => (IFunctionHandler)serviceProvider.GetService(typeof(YesterdayWinLossFunctionHandler)),
                FunctionIds.AvailableDomains => (IFunctionHandler)serviceProvider.GetService(typeof(AvailableDomainsFunctionHandler)),
             _ => throw new NotImplementedException(),
            };
        }
    }
}