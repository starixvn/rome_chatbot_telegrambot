using System.Threading.Tasks;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class BaseFunctionHandler
    {
        protected readonly IBuilderService builderService;
        protected readonly ISenderService senderService;

        public BaseFunctionHandler(IBuilderService builderService, ISenderService senderService)
        {
            this.builderService = builderService;
            this.senderService = senderService;
        }

        protected async Task BuildAndSendInvalidCustNameMessage(CustBotMessenger messenger, string invalidCustName)
        {
            var message = builderService.BuildInvalidCustNameMessage(messenger.Language, invalidCustName);

            await senderService.SendMessage(messenger, message);
        }

        protected async Task BuildAndSendInvalidNumerOfDownlineRangeMessage(CustBotMessenger messenger, int maxTopQuery)
            => await senderService.SendMessage(
                messenger,
                builderService.BuildInvalidNumerOfDownlineRange(messenger.Language, maxTopQuery));

        protected async Task BuildAndSendNoDataMessage(CustBotMessenger messenger)
            => await senderService.SendMessage(messenger, ResourceHelper.Get(nameof(SharedResource.NoData), messenger.Language));



        protected static bool IsValidDirectDownline(int custLevel, DownlineInfo downlineInfo)
            => downlineInfo != null
                && downlineInfo.CustId > 0
                && IsDirectDownline(custLevel, downlineInfo.RoleId);

        private static bool IsDirectDownline(int custLevel, int downlineLevel)
            => (custLevel - downlineLevel) == 1;
    }
}