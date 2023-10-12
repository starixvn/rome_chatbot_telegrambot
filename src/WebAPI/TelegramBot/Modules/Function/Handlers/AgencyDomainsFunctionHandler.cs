using Fanex.Data.Repository;
using TelegramBot.Shared.Models;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Modules.Message;

namespace TelegramBot.Modules.Function.Handlers
{
    public class AgencyDomainsFunctionHandler : AvailableDomainsFunctionHandler, IFunctionHandler
    {
        public AgencyDomainsFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IDynamicRepository dynamicRepository) : base(messageBuilder, messageFormatter, dynamicRepository)
        {
        }

        public override Task<Result> Handle(CustBotAccount account, string[] parameters)
            => base.Handle(account, new[] { "1" });
    }
}