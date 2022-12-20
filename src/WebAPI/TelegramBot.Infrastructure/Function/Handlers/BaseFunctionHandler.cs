using Alpha.API.Client.Models.Customers;
using TelegramBot.Infrastructure.Message;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class BaseFunctionHandler
    {
        protected const int ColumnsWidth = 15;
        protected const int NoColumnWidth = 3;
        protected const int ColumnsWithNoWidth = 13;
        protected readonly IMessageBuilder messageBuilder;
        protected readonly IMessageFormatter messageFormatter;

        public BaseFunctionHandler(IMessageBuilder messageBuilder, IMessageFormatter messageFormatter)
        {
            this.messageBuilder = messageBuilder;
            this.messageFormatter = messageFormatter;
        }

        protected static bool IsValidDirectDownline(int custLevel, DownlineInfo downlineInfo)
            => downlineInfo != null
                && downlineInfo.CustId > 0
                && IsDirectDownline(custLevel, downlineInfo.RoleId);

        private static bool IsDirectDownline(int custLevel, int downlineLevel)
            => (custLevel - downlineLevel) == 1;
    }
}