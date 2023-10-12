using TelegramBot.Modules.Function.Constants;

namespace TelegramBot.Modules.Function.Handlers
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
                FunctionIds.AgencyDomains => (IFunctionHandler)serviceProvider.GetService(typeof(AgencyDomainsFunctionHandler)),
                FunctionIds.MemberDomains => (IFunctionHandler)serviceProvider.GetService(typeof(MemberDomainsFunctionHandler)),
                _ => throw new NotImplementedException(),
            };
        }
    }
}