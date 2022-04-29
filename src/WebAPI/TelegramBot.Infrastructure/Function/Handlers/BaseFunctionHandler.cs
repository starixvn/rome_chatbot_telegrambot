using System.Threading.Tasks;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class BaseFunctionHandler
    {
        protected readonly IMessageBuilder messageBuilder;
        protected readonly IMessageSender messageSender;
        protected readonly IMessageFormatter messageFormatter;

        public BaseFunctionHandler(IMessageBuilder messageBuilder, IMessageSender messageSender, IMessageFormatter messageFormatter)
        {
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.messageFormatter = messageFormatter;
        }

        protected async Task BuildAndSendInvalidCustNameMessage(CustBotMessenger messenger, string invalidCustName)
        {
            var message = messageBuilder.BuildInvalidCustNameMessage(messenger.Language, invalidCustName);

            await messageSender.SendMessage(messenger, message);
        }

        protected async Task BuildAndSendInvalidNumerOfDownlineRangeMessage(CustBotMessenger messenger, int maxTopQuery)
            => await messageSender.SendMessage(
                messenger,
                messageBuilder.BuildInvalidNumerOfDownlineRange(messenger.Language, maxTopQuery));

        protected async Task BuildAndSendNoDataMessage(CustBotMessenger messenger)
            => await messageSender.SendMessage(messenger, ResourceHelper.Get(nameof(SharedResource.NoData), messenger.Language));



        protected static bool IsValidDirectDownline(int custLevel, DownlineInfo downlineInfo)
            => downlineInfo != null
                && downlineInfo.CustId > 0
                && IsDirectDownline(custLevel, downlineInfo.RoleId);

        private static bool IsDirectDownline(int custLevel, int downlineLevel)
            => (custLevel - downlineLevel) == 1;
    }
}