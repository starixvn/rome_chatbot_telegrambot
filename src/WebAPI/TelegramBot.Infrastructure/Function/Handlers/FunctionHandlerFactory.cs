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

                default:
                    throw new NotImplementedException();
            }
        }
    }
}