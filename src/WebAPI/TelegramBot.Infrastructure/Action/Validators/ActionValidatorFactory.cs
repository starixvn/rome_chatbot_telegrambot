using System;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Action.Validators
{
    public interface IActionValidatorFactory
    {
        IActionValidator GetValidator(short actionId);
    }

    public class ActionValidatorFactory : IActionValidatorFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ActionValidatorFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IActionValidator GetValidator(short actionId)
        {
            switch (actionId)
            {
                case ActionIds.OutstandingEnterDownlineUsername:
                    return new DirectDownlinesValidator(
                        (IMessageBuilder)serviceProvider.GetService(typeof(IMessageBuilder)),
                        (ICustomerService)serviceProvider.GetService(typeof(ICustomerService)));
                case ActionIds.TopXOutstanding:
                    return new NumberOfDownlinesValidator(
                        (IMessageBuilder)serviceProvider.GetService(typeof(IMessageBuilder)),
                        ((WebAPIConfiguration)serviceProvider.GetService(typeof(WebAPIConfiguration))).OutstandingMaxTopQuery);
                default:
                    return null;
            }
        }
    }
}